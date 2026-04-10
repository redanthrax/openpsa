using Common.Caching;
using Common.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Common.Audit;

public partial class AuditPurgeBackgroundService : BackgroundService {
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IDistributedLockService _lockService;
    private readonly ILogger<AuditPurgeBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24);
    private readonly int _retentionDays = 90;
    private const string LockKey = "background:audit-purge";

    public AuditPurgeBackgroundService(
        IServiceScopeFactory scopeFactory,
        IDistributedLockService lockService,
        ILogger<AuditPurgeBackgroundService> logger) {
        _scopeFactory = scopeFactory;
        _lockService = lockService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        LogServiceStarted(_logger, _retentionDays);
        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken).ConfigureAwait(false);

        while (!stoppingToken.IsCancellationRequested) {
            try {
                await using var lockHandle = await _lockService.TryAcquireAsync(LockKey, TimeSpan.FromMinutes(10), stoppingToken).ConfigureAwait(false);
                if (lockHandle != null) {
                    await PurgeOldAuditEntriesAsync(stoppingToken).ConfigureAwait(false);
                }
            } catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) {
                break;
            } catch (Exception ex) {
                LogPurgeError(_logger, ex);
            }

            try {
                await Task.Delay(_checkInterval, stoppingToken).ConfigureAwait(false);
            } catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) {
                break;
            }
        }

        LogServiceStopped(_logger);
    }

    private async Task PurgeOldAuditEntriesAsync(CancellationToken cancellationToken) {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OpenPsaDbContext>();
        var cutoffDate = DateTime.UtcNow.AddDays(-_retentionDays);
        var deletedCount = await dbContext.Set<AuditEntry>()
            .Where(a => a.CreatedAt < cutoffDate)
            .ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
        if (deletedCount > 0) LogPurgedEntries(_logger, deletedCount, _retentionDays);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Audit purge background service started with {RetentionDays}-day retention")]
    private static partial void LogServiceStarted(ILogger logger, int retentionDays);

    [LoggerMessage(Level = LogLevel.Information, Message = "Audit purge background service stopped")]
    private static partial void LogServiceStopped(ILogger logger);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error purging audit entries")]
    private static partial void LogPurgeError(ILogger logger, Exception ex);

    [LoggerMessage(Level = LogLevel.Information, Message = "Purged {Count} audit entries older than {RetentionDays} days")]
    private static partial void LogPurgedEntries(ILogger logger, int count, int retentionDays);
}
