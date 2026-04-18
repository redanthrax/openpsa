using Common.Database;
using IntegrationEvents.Email;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Email.Models;

namespace OpenPsa.Modules.Email.Features.Integration;

public class EmailQueryHandler {
    public static async Task<GetMailboxConnectionResponse> Handle(
        GetMailboxConnectionQuery query, OpenPsaDbContext db) {
        var c = await db.Set<MailboxConnection>().FindAsync(query.MailboxConnectionId);
        if (c is null)
            return new GetMailboxConnectionResponse(false, Guid.Empty, string.Empty, null, null, false, null, null, null, null, null, null, default);

        return new GetMailboxConnectionResponse(true, c.Id, c.EmailAddress, c.SmtpHost, c.SmtpPort, c.SmtpUseSsl,
            c.SmtpUsername, c.EncryptedSmtpPassword, c.GraphTenantId, c.GraphClientId, c.EncryptedGraphClientSecret,
            c.GraphMailboxUserId, c.Provider);
    }

    public static async Task<FindMailboxByEmailResponse> Handle(
        FindMailboxByEmailQuery query, OpenPsaDbContext db) {
        var c = await db.Set<MailboxConnection>()
            .FirstOrDefaultAsync(m => m.EmailAddress == query.EmailAddress);
        if (c is null)
            return new FindMailboxByEmailResponse(false, null, null, false, default);

        return new FindMailboxByEmailResponse(true, c.Id, c.DefaultQueueId, c.AutoCreateContacts, c.Provider);
    }

    public static async Task<GetMailboxForTicketResponse> Handle(
        GetMailboxForTicketQuery query, OpenPsaDbContext db) {
        var msg = await db.Set<EmailMessage>()
            .Where(e => e.TicketId == query.TicketId)
            .OrderByDescending(e => e.CreatedAt)
            .FirstOrDefaultAsync();

        if (msg is null)
            return new GetMailboxForTicketResponse(false, null);

        return new GetMailboxForTicketResponse(true, msg.MailboxConnectionId);
    }
}
