namespace SaleManagement.Api.Services;

public sealed record RateLimitDecision(
    bool IsAllowed,
    int Limit,
    int Remaining,
    int ResetSeconds,
    int RetryAfterSeconds
);

public interface IRateLimitStore
{
    Task<RateLimitDecision> ConsumeAsync(
        string key,
        int capacity,
        TimeSpan refillPeriod,
        CancellationToken cancellationToken);
}