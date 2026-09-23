using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using SaleManagement.Api.Middleware;
using SaleManagement.Api.Options;
using SaleManagement.Api.Services;
using Xunit;

namespace SaleManagement.Api.Tests.Middleware;

public sealed class RedisRateLimitingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_Returns429_WhenBucketIsEmpty()
    {
        var store = new Mock<IRateLimitStore>();
        store.Setup(value => value.ConsumeAsync(
                It.IsAny<string>(),
                60,
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RateLimitDecision(false, 60, 0, 2, 2));
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Loopback;
        context.Response.Body = new MemoryStream();
        var nextCalled = false;
        var middleware = new RedisRateLimitingMiddleware(
            _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            },
            store.Object,
            Microsoft.Extensions.Options.Options.Create(new RateLimitOptions()),
            NullLogger<RedisRateLimitingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status429TooManyRequests, context.Response.StatusCode);
        Assert.Equal("60", context.Response.Headers["X-RateLimit-Limit"]);
        Assert.Equal("0", context.Response.Headers["X-RateLimit-Remaining"]);
        Assert.Equal("2", context.Response.Headers.RetryAfter);
        Assert.False(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_SkipsHealthProbe()
    {
        var store = new Mock<IRateLimitStore>();
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/healthz";
        var nextCalled = false;
        var middleware = new RedisRateLimitingMiddleware(
            _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            },
            store.Object,
            Microsoft.Extensions.Options.Options.Create(new RateLimitOptions()),
            NullLogger<RedisRateLimitingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
        store.Verify(value => value.ConsumeAsync(
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }
}