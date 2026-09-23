using StackExchange.Redis;
using System.Globalization;

namespace SaleManagement.Api.Services;

public sealed class RedisRateLimitStore : IRateLimitStore
{
    private const string TokenBucketScript = """
        local now = tonumber(ARGV[1])
        local capacity = tonumber(ARGV[2])
        local refill = tonumber(ARGV[3])
        local cost = tonumber(ARGV[4])
        local values = redis.call('HMGET', KEYS[1], 'tokens', 'updated')
        local tokens = tonumber(values[1])
        local updated = tonumber(values[2])

        if not tokens or not updated then
            tokens = capacity
            updated = now
        end

        tokens = math.min(capacity, tokens + math.max(0, now - updated) / 1000 * refill)
        local allowed = 0
        if tokens >= cost then
            tokens = tokens - cost
            allowed = 1
        end

        redis.call('HSET', KEYS[1], 'tokens', tokens, 'updated', now)
        redis.call('EXPIRE', KEYS[1], math.ceil(capacity / refill) + 60)

        local retry = 0
        if allowed == 0 then
            retry = math.ceil((cost - tokens) / refill)
        end
        local reset = math.ceil((capacity - tokens) / refill)
        return tostring(allowed) .. '|' .. tostring(tokens) .. '|' .. tostring(reset) .. '|' .. tostring(retry)
        """;

    private readonly IConnectionMultiplexer _connection;

    public RedisRateLimitStore(IConnectionMultiplexer connection)
    {
        _connection = connection;
    }

    public async Task<RateLimitDecision> ConsumeAsync(
        string key,
        int capacity,
        TimeSpan refillPeriod,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var database = _connection.GetDatabase();
        var refillPerSecond = capacity / refillPeriod.TotalSeconds;
        var result = await database.ScriptEvaluateAsync(
            TokenBucketScript,
            new[] { (RedisKey)key },
            new RedisValue[]
            {
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                capacity,
                refillPerSecond,
                1
            }).ConfigureAwait(false);

        var values = result.ToString().Split('|');
        var allowed = values.Length == 4 && values[0] == "1";
        var remaining = values.Length == 4
            ? Math.Max(0, (int)Math.Floor(double.Parse(values[1], CultureInfo.InvariantCulture)))
            : 0;
        var reset = values.Length == 4
            ? int.Parse(values[2], CultureInfo.InvariantCulture)
            : (int)refillPeriod.TotalSeconds;
        var retry = values.Length == 4
            ? int.Parse(values[3], CultureInfo.InvariantCulture)
            : 1;
        return new RateLimitDecision(allowed, capacity, remaining, reset, retry);
    }
}