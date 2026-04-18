using Common.Database;
using Contracts.Email;
using Contracts.Tickets;
using IntegrationEvents.Clients;
using IntegrationEvents.Contacts;
using IntegrationEvents.Email;
using IntegrationEvents.Tickets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenPsa.Modules.Email.Models;
using Wolverine;

namespace OpenPsa.Modules.Email.Services;

public class InboundEmailProcessor {
    private readonly OpenPsaDbContext _db;
    private readonly IMessageBus _bus;
    private readonly ILogger<InboundEmailProcessor> _logger;

    public InboundEmailProcessor(OpenPsaDbContext db, IMessageBus bus, ILogger<InboundEmailProcessor> logger) {
        _db = db;
        _bus = bus;
        _logger = logger;
    }

    public async Task ProcessInboundEmailAsync(EmailMessage email, MailboxConnection mailbox, CancellationToken ct) {
        if (IsAutoReply(email)) {
            _logger.LogDebug("Skipping auto-reply email {MessageId}", email.MessageId);
            return;
        }

        var existingTicketId = await MatchToExistingTicketAsync(email, ct);
        if (existingTicketId.HasValue) {
            email.TicketId = existingTicketId.Value;
            _logger.LogInformation("Matched email {MessageId} to ticket {TicketId}", email.MessageId, existingTicketId.Value);
            await _bus.PublishAsync(new EmailReceived(email.Id, email.MailboxConnectionId, email.FromAddress, email.Subject, email.TicketId));
            return;
        }

        var ticketId = await CreateTicketFromEmailAsync(email, mailbox, ct);
        email.TicketId = ticketId;
        _logger.LogInformation("Created ticket {TicketId} from email {MessageId}", ticketId, email.MessageId);
        await _bus.PublishAsync(new EmailReceived(email.Id, email.MailboxConnectionId, email.FromAddress, email.Subject, email.TicketId));
    }

    private async Task<Guid?> MatchToExistingTicketAsync(EmailMessage email, CancellationToken ct) {
        var ticketId = ExtractTicketIdFromSubject(email.Subject);
        if (ticketId.HasValue) {
            var exists = await _db.Set<OpenPsa.Modules.Email.Models.EmailMessage>()
                .AnyAsync(e => e.TicketId == ticketId.Value, ct);
            if (exists) return ticketId.Value;
        }

        if (!string.IsNullOrEmpty(email.InReplyTo)) {
            var original = await _db.Set<EmailMessage>()
                .Where(e => e.MessageId == email.InReplyTo && e.TicketId != null)
                .Select(e => e.TicketId)
                .FirstOrDefaultAsync(ct);
            if (original.HasValue) return original.Value;
        }

        if (!string.IsNullOrEmpty(email.References)) {
            var refs = email.References.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var reference in refs) {
                var match = await _db.Set<EmailMessage>()
                    .Where(e => e.MessageId == reference && e.TicketId != null)
                    .Select(e => e.TicketId)
                    .FirstOrDefaultAsync(ct);
                if (match.HasValue) return match.Value;
            }
        }

        return null;
    }

    private async Task<Guid> CreateTicketFromEmailAsync(EmailMessage email, MailboxConnection mailbox, CancellationToken ct) {
        var (clientId, contactId) = await ResolveClientAndContactAsync(email, mailbox, ct);
        email.ClientId = clientId;
        email.ContactId = contactId;

        var request = new CreateTicketRequest {
            Title = string.IsNullOrWhiteSpace(email.Subject) ? "Email (no subject)" : email.Subject,
            Description = email.BodyText ?? StripHtml(email.BodyHtml),
            Priority = TicketPriority.Medium,
            Type = TicketType.Incident,
            ClientId = clientId,
            QueueId = mailbox.DefaultQueueId
        };

        var response = await _bus.InvokeAsync<TicketCreatedResponse>(
            new CreateTicketFromEmailCommand(request.Title, request.Description, request.Priority,
                request.Type, request.ClientId, request.QueueId), ct);

        return response.TicketId;
    }

    private async Task<(Guid ClientId, Guid? ContactId)> ResolveClientAndContactAsync(
        EmailMessage email, MailboxConnection mailbox, CancellationToken ct) {
        var contactResponse = await _bus.InvokeAsync<FindClientByContactEmailResponse>(
            new FindClientByContactEmailQuery(email.FromAddress), ct);

        if (contactResponse.Found && contactResponse.ClientId.HasValue)
            return (contactResponse.ClientId.Value, null);

        var defaultResponse = await _bus.InvokeAsync<GetDefaultClientResponse>(
            new GetDefaultClientQuery(), ct);

        var clientId = defaultResponse.ClientId;

        if (mailbox.AutoCreateContacts) {
            try {
                var createResponse = await _bus.InvokeAsync<CreateContactFromEmailResponse>(
                    new CreateContactFromEmailCommand(email.FromAddress, email.FromName, clientId), ct);
                _logger.LogInformation("Auto-created contact {ContactId} for {Email}", createResponse.ContactId, email.FromAddress);
                return (clientId, createResponse.ContactId);
            } catch (Exception ex) {
                _logger.LogWarning(ex, "Failed to auto-create contact for {Email}", email.FromAddress);
            }
        }

        return (clientId, null);
    }

    private static Guid? ExtractTicketIdFromSubject(string? subject) {
        if (string.IsNullOrEmpty(subject)) return null;
        var match = System.Text.RegularExpressions.Regex.Match(subject,
            @"\[Ticket #([0-9a-fA-F-]{36})\]");
        return match.Success && Guid.TryParse(match.Groups[1].Value, out var id) ? id : null;
    }

    private static bool IsAutoReply(EmailMessage email) {
        if (string.IsNullOrEmpty(email.Subject)) return false;
        var subject = email.Subject.ToLowerInvariant();
        return subject.StartsWith("auto:") || subject.StartsWith("automatic reply:") ||
               subject.Contains("out of office") || subject.Contains("auto-reply");
    }

    private static string? StripHtml(string? html) {
        if (string.IsNullOrEmpty(html)) return null;
        return System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", " ").Trim();
    }
}
