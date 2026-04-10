using Common.Authorization;
using Common.Caching;
using Common.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenPsa.Modules.Authentication.Models;

namespace OpenPsa.Modules.Authentication.Services;

public partial class PermissionSyncHostedService : IHostedService {
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PermissionSyncHostedService> _logger;

    public PermissionSyncHostedService(IServiceProvider serviceProvider, ILogger<PermissionSyncHostedService> logger) {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken) {
        try {
            using var scope = _serviceProvider.CreateScope();
            var lockService = scope.ServiceProvider.GetRequiredService<IDistributedLockService>();

            await using var lockHandle = await lockService.TryAcquireAsync(
                "startup:permission-sync", TimeSpan.FromMinutes(2), cancellationToken).ConfigureAwait(false);

            if (lockHandle == null) {
                LogPermissionSyncSkipped(_logger);
                return;
            }

            var dbContext = scope.ServiceProvider.GetRequiredService<OpenPsaDbContext>();
            var registry = scope.ServiceProvider.GetRequiredService<IPermissionRegistry>();

            var registeredPermissions = registry.GetAll().ToList();
            var existingPermissions = await dbContext.Set<Permission>().ToListAsync(cancellationToken).ConfigureAwait(false);

            var newPermissions = new List<Permission>();
            var updatedCount = 0;

            foreach (var registered in registeredPermissions) {
                var existing = existingPermissions.FirstOrDefault(p => p.Key == registered.Key);
                if (existing == null) {
                    newPermissions.Add(new Permission {
                        Key = registered.Key,
                        Name = registered.Name,
                        Description = registered.Description,
                        Category = registered.Category,
                        CreatedAt = DateTime.UtcNow
                    });
                } else if (existing.Name != registered.Name || existing.Description != registered.Description || existing.Category != registered.Category) {
                    existing.Name = registered.Name;
                    existing.Description = registered.Description;
                    existing.Category = registered.Category;
                    updatedCount++;
                }
            }

            if (newPermissions.Count > 0)
                dbContext.Set<Permission>().AddRange(newPermissions);

            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            LogPermissionSync(_logger, newPermissions.Count, updatedCount);
        } catch (Exception ex) {
            LogPermissionSyncError(_logger, ex);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    [LoggerMessage(Level = LogLevel.Information, Message = "Permission sync completed. New: {NewCount}, Updated: {UpdatedCount}")]
    private static partial void LogPermissionSync(ILogger logger, int newCount, int updatedCount);

    [LoggerMessage(Level = LogLevel.Information, Message = "Permission sync skipped — another instance is handling it")]
    private static partial void LogPermissionSyncSkipped(ILogger logger);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error syncing permissions on startup")]
    private static partial void LogPermissionSyncError(ILogger logger, Exception exception);
}
