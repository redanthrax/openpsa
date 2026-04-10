using StackExchange.Redis;

namespace Common.Caching;

public class RedisDistributedLockService : IDistributedLockService {
    private readonly IConnectionMultiplexer _redis;

    public RedisDistributedLockService(IConnectionMultiplexer redis) {
        _redis = redis;
    }

    public async Task<IAsyncDisposable?> TryAcquireAsync(string key, TimeSpan expiry, CancellationToken cancellationToken = default) {
        var db = _redis.GetDatabase();
        var lockKey = $"lock:{key}";
        var lockValue = Guid.NewGuid().ToString();
        var acquired = await db.StringSetAsync(lockKey, lockValue, expiry, When.NotExists).ConfigureAwait(false);
        if (!acquired) return null;
        return new RedisLockHandle(db, lockKey, lockValue);
    }

    private sealed class RedisLockHandle : IAsyncDisposable {
        private readonly IDatabase _db;
        private readonly string _key;
        private readonly string _value;

        public RedisLockHandle(IDatabase db, string key, string value) {
            _db = db;
            _key = key;
            _value = value;
        }

        public async ValueTask DisposeAsync() {
            var current = await _db.StringGetAsync(_key).ConfigureAwait(false);
            if (current == _value) {
                await _db.KeyDeleteAsync(_key).ConfigureAwait(false);
            }
        }
    }
}
