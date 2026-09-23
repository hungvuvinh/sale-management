using System.Security.Claims;
using Microsoft.Extensions.Options;
using SaleManagement.Api.Options;
using SaleManagement.Api.Services;
using StackExchange.Redis;

namespace SaleManagement.Api.Middleware;

public sealed class RedisRateLimitingMiddleware
{
    private static readonly TimeSpan RefillPeriod = TimeSpan.FromMinutes(1);
    private readonly RequestDelegate _next;
    private readonly IRateLimitStore _rateLimitStore;
    private readonly RateLimitOptions _options;
    private readonly ILogger<RedisRateLimitingMiddleware> _logger;

    public RedisRateLimitingMiddleware(
        RequestDelegate next,
        IRateLimitStore rateLimitStore,
        IOptions<RateLimitOptions> options,
        ILogger<RedisRateLimitingMiddleware> logger)
    {
        _next = next;
        _rateLimitStore = rateLimitStore;
        _options = options.Value;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (IsHealthProbe(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var (identity, capacity) = GetIdentityAndCapacity(context);
        var key = $"rate-limit:{identity}";
        RateLimitDecision decision;
        try
        {
            decision = await _rateLimitStore.ConsumeAsync(key, capacity, RefillPeriod, context.RequestAborted);
        }
        catch (Exception exception) when (exception is RedisConnectionException or RedisTimeoutException)
        {
            _logger.LogWarning(exception, "Redis rate limiter unavailable for {RateLimitKey}", key);
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = "Rate limiter tijdelijk niet beschikbaar. Probeer het opnieuw."
            });
            return;
        }

        AddHeaders(context.Response, decision);
        if (!decision.IsAllowed)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.ContentType = "application/json";
            context.Response.Headers.RetryAfter = decision.RetryAfterSeconds.ToString();
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = "Quá số lượng truy cập cho phép. Vui lòng thử lại sau.",
                retryAfter = decision.RetryAfterSeconds
            });
            return;
        }

        await _next(context);
    }

    private (string Identity, int Capacity) GetIdentityAndCapacity(HttpContext context)
    {
        var userId = context.User.FindFirstValue("user_id") ?? context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrWhiteSpace(userId))
        {
            var role = context.User.FindFirstValue(ClaimTypes.Role) ?? context.User.FindFirstValue("role");
            var capacity = role is "SUPER_ADMIN" or "ADMIN" or "STORE_MANAGER"
                ? _options.AdminRequestsPerMinute
                : _options.StaffRequestsPerMinute;
            return ($"user:{userId}", capacity);
        }

        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return ($"ip:{ip}", _options.PublicRequestsPerMinute);
    }

    private static bool IsHealthProbe(PathString path) =>
        path.Equals("/api/healthz", StringComparison.OrdinalIgnoreCase) ||
        path.Equals("/api/readyz", StringComparison.OrdinalIgnoreCase);

    private static void AddHeaders(HttpResponse response, RateLimitDecision decision)
    {
        response.Headers["X-RateLimit-Limit"] = decision.Limit.ToString();
        response.Headers["X-RateLimit-Remaining"] = decision.Remaining.ToString();
        response.Headers["X-RateLimit-Reset"] = decision.ResetSeconds.ToString();
    }
}