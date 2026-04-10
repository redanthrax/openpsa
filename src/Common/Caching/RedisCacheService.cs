using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace Common.Caching;

public class RedisCacheService : ICacheService {
    private readonly IDistributedCache _cache;
    private readonly IConnectionMultiplexer _redis;
    private readonly JsonSerializerOptions _jsonOptions;

    public RedisCacheService(IDistributedCache cache, IConnectionMultiplexer redis) {
        _cache = cache;
        _redis = redis;
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class {
        var cachedData = await _cache.GetStringAsync(key, cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrEmpty(cachedData)) return null;
        return JsonSerializer.Deserialize<T>(cachedData, _jsonOptions);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class {
        var serializedData = JsonSerializer.Serialize(value, _jsonOptions);
        var options = new DistributedCacheEntryOptions();
        if (expiration.HasValue) options.AbsoluteExpirationRelativeToNow = expiration.Value;
        await _cache.SetStringAsync(key, serializedData, options, cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default) {
        await _cache.RemoveAsync(key, cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default) {
        var db = _redis.GetDatabase();
        foreach (var endpoint in _redis.GetEndPoints()) {
            var server = _redis.GetServer(endpoint);
            await foreach (var key in server.KeysAsync(pattern: $"{CacheExtensions.InstanceName}{pattern}").WithCancellation(cancellationToken)) {
                await db.KeyDeleteAsync(key).ConfigureAwait(false);
            }
        }
    }
}
