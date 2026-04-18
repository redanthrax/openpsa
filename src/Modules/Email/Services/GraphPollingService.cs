using Common.Caching;
using Common.Database;
using Contracts.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenPsa.Modules.Email.Models;

namespace OpenPsa.Modules.Email.Services;

public class GraphPollingService : BackgroundService {
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<GraphPollingService> _logger;

    public GraphPollingService(IServiceScopeFactory scopeFactory, ILogger<GraphPollingService> logger) {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        while (!stoppingToken.IsCancellationRequested) {
            try {
                await PollAllMailboxesAsync(stoppingToken);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error polling Graph mailboxes");
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    private async Task PollAllMailboxesAsync(CancellationToken ct) {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpenPsaDbContext>();
        var lockService = scope.ServiceProvider.GetRequiredService<IDistributedLockService>();

        await using var lockHandle = await lockService.TryAcquireAsync("background:graph-poll", TimeSpan.FromMinutes(5), ct);
        if (lockHandle == null) return;

        var mailboxes = await db.Set<MailboxConnection>()
            .Where(m => m.Status == MailboxConnectionStatus.Active && m.Provider == MailboxProvider.MicrosoftGraph)
            .ToListAsync(ct);

        foreach (var mailbox in mailboxes) {
            if (ct.IsCancellationRequested) break;
            if (mailbox.LastPollAt.HasValue &&
                (DateTime.UtcNow - mailbox.LastPollAt.Value).TotalSeconds < mailbox.PollIntervalSeconds)
                continue;

            try {
                await PollMailboxAsync(db, scope.ServiceProvider, mailbox, ct);
                mailbox.LastPollAt = DateTime.UtcNow;
                mailbox.LastError = null;
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error polling Graph mailbox {MailboxId} ({Email})", mailbox.Id, mailbox.EmailAddress);
                mailbox.LastError = ex.Message;
                mailbox.Status = MailboxConnectionStatus.Error;
            }
        }

        await db.SaveChangesAsync(ct);
    }

    private async Task PollMailboxAsync(OpenPsaDbContext db, IServiceProvider sp, MailboxConnection mailbox, CancellationToken ct) {
        var graphService = sp.GetRequiredService<GraphMailService>();
        var processor = sp.GetRequiredService<InboundEmailProcessor>();

        var (messages, deltaLink) = await graphService.PollMessagesAsync(mailbox, ct);

        foreach (var emailMsg in messages) {
            var exists = await db.Set<EmailMessage>()
                .AnyAsync(e => e.MessageId == emailMsg.MessageId && e.MailboxConnectionId == mailbox.Id, ct);
            if (exists) continue;

            db.Set<EmailMessage>().Add(emailMsg);
            mailbox.MessageCount++;

            await processor.ProcessInboundEmailAsync(emailMsg, mailbox, ct);
        }

        if (deltaLink != null)
            mailbox.GraphDeltaLink = deltaLink;
    }
}
