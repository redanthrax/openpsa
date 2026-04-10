namespace Common.Caching;

public interface IDistributedLockService {
    Task<IAsyncDisposable?> TryAcquireAsync(string key, TimeSpan expiry, CancellationToken cancellationToken = default);
}
