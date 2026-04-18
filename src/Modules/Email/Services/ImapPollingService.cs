using Common.Caching;
using Common.Database;
using Common.Security;
using Contracts.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenPsa.Modules.Email.Models;

namespace OpenPsa.Modules.Email.Services;

public class ImapPollingService : BackgroundService {
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ImapPollingService> _logger;

    public ImapPollingService(IServiceScopeFactory scopeFactory, ILogger<ImapPollingService> logger) {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        while (!stoppingToken.IsCancellationRequested) {
            try {
                await PollAllMailboxesAsync(stoppingToken);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error polling IMAP mailboxes");
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    private async Task PollAllMailboxesAsync(CancellationToken ct) {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpenPsaDbContext>();
        var lockService = scope.ServiceProvider.GetRequiredService<IDistributedLockService>();

        await using var lockHandle = await lockService.TryAcquireAsync("background:email-poll", TimeSpan.FromMinutes(5), ct);
        if (lockHandle == null) return;

        var mailboxes = await db.Set<MailboxConnection>()
            .Where(m => m.Status == MailboxConnectionStatus.Active && m.Provider == MailboxProvider.Imap)
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
                _logger.LogError(ex, "Error polling mailbox {MailboxId} ({Email})", mailbox.Id, mailbox.EmailAddress);
                mailbox.LastError = ex.Message;
                mailbox.Status = MailboxConnectionStatus.Error;
            }
        }

        await db.SaveChangesAsync(ct);
    }

    private async Task PollMailboxAsync(OpenPsaDbContext db, IServiceProvider sp, MailboxConnection mailbox, CancellationToken ct) {
        var piiService = sp.GetService<IPiiEncryptionService>();
        if (string.IsNullOrEmpty(mailbox.ImapHost) || string.IsNullOrEmpty(mailbox.EncryptedImapPassword))
            return;

        var password = piiService?.Decrypt(mailbox.EncryptedImapPassword) ?? mailbox.EncryptedImapPassword;

        using var client = new MailKit.Net.Imap.ImapClient();
        await client.ConnectAsync(mailbox.ImapHost, mailbox.ImapPort ?? 993, mailbox.ImapUseSsl, ct);
        await client.AuthenticateAsync(mailbox.ImapUsername ?? mailbox.EmailAddress, password, ct);

        var inbox = client.Inbox;
        await inbox.OpenAsync(MailKit.FolderAccess.ReadWrite, ct);

        var uids = await GetNewUidsAsync(inbox, mailbox.LastSyncUid, ct);

        foreach (var uid in uids) {
            var message = await inbox.GetMessageAsync(uid, ct);
            var messageId = message.MessageId;

            var exists = await db.Set<EmailMessage>()
                .AnyAsync(e => e.MessageId == messageId && e.MailboxConnectionId == mailbox.Id, ct);
            if (exists) continue;

            var emailMsg = new EmailMessage {
                MailboxConnectionId = mailbox.Id,
                Direction = EmailDirection.Inbound,
                DeliveryStatus = EmailDeliveryStatus.Received,
                FromAddress = message.From.Mailboxes.FirstOrDefault()?.Address ?? string.Empty,
                FromName = message.From.Mailboxes.FirstOrDefault()?.Name ?? string.Empty,
                ToAddress = message.To.Mailboxes.FirstOrDefault()?.Address ?? mailbox.EmailAddress,
                Subject = message.Subject ?? string.Empty,
                BodyHtml = message.HtmlBody,
                BodyText = message.TextBody,
                MessageId = messageId,
                InReplyTo = message.InReplyTo,
                References = message.References != null ? string.Join(" ", message.References) : null,
                AttachmentCount = message.Attachments.Count(),
                SentAt = message.Date.UtcDateTime
            };

            db.Set<EmailMessage>().Add(emailMsg);
            mailbox.MessageCount++;
            mailbox.LastSyncUid = uid.Id.ToString();

            var processor = sp.GetRequiredService<InboundEmailProcessor>();
            await processor.ProcessInboundEmailAsync(emailMsg, mailbox, ct);

            await inbox.StoreAsync(uid, new MailKit.StoreFlagsRequest(MailKit.StoreAction.Add, MailKit.MessageFlags.Seen) { Silent = true }, ct);
        }

        await client.DisconnectAsync(true, ct);
    }

    private static async Task<IList<MailKit.UniqueId>> GetNewUidsAsync(
        MailKit.IMailFolder inbox, string? lastSyncUid, CancellationToken ct) {
        if (string.IsNullOrEmpty(lastSyncUid) || !uint.TryParse(lastSyncUid, out var lastUid)) {
            var range = new MailKit.Search.SearchQuery();
            var allUids = await inbox.SearchAsync(MailKit.Search.SearchQuery.NotSeen, ct);
            return allUids.Take(50).ToList();
        }

        var nextUid = new MailKit.UniqueId(lastUid + 1);
        var uidRange = new MailKit.UniqueIdRange(nextUid, MailKit.UniqueId.MaxValue);
        return (await inbox.SearchAsync(uidRange, MailKit.Search.SearchQuery.All, ct)).ToList();
    }
}
