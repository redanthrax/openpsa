using Common.Caching;
using Common.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenPsa.Modules.Sla.Models;

namespace OpenPsa.Modules.Sla.Services;

public class SlaBreachCheckerService : BackgroundService {
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SlaBreachCheckerService> _logger;

    public SlaBreachCheckerService(IServiceScopeFactory scopeFactory, ILogger<SlaBreachCheckerService> logger) {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        while (!stoppingToken.IsCancellationRequested) {
            try {
                await CheckBreachesAsync(stoppingToken);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error checking SLA breaches");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task CheckBreachesAsync(CancellationToken ct) {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpenPsaDbContext>();
        var lockService = scope.ServiceProvider.GetRequiredService<IDistributedLockService>();

        await using var lockHandle = await lockService.TryAcquireAsync("background:sla-breach-check", TimeSpan.FromMinutes(2), ct);
        if (lockHandle == null) return;

        var now = DateTime.UtcNow;

        var responseBreaches = await db.Set<SlaInstance>()
            .Where(i => !i.IsPaused && !i.ResponseBreached && i.RespondedAt == null && i.ResponseDueAt <= now)
            .ToListAsync(ct);

        foreach (var instance in responseBreaches)
            instance.ResponseBreached = true;

        var resolutionBreaches = await db.Set<SlaInstance>()
            .Where(i => !i.IsPaused && !i.ResolutionBreached && i.ResolvedAt == null && i.ResolutionDueAt <= now)
            .ToListAsync(ct);

        foreach (var instance in resolutionBreaches)
            instance.ResolutionBreached = true;

        if (responseBreaches.Count > 0 || resolutionBreaches.Count > 0) {
            await db.SaveChangesAsync(ct);
            _logger.LogInformation("SLA breaches detected: {ResponseBreaches} response, {ResolutionBreaches} resolution",
                responseBreaches.Count, resolutionBreaches.Count);
        }
    }
}
