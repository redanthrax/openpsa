using Common.Authorization;
using Common.Database;
using Common.Modules;
using Common.Security;
using Contracts.Email;
using Contracts.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OpenPsa.Modules.Email.Models;

namespace OpenPsa.Modules.Email.Features.CreateMailboxConnection;

public class CreateMailboxConnectionEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapPost("/api/mailbox-connections", async (
            CreateMailboxConnectionRequest request, OpenPsaDbContext db, IPiiEncryptionService pii) => {

            var connection = new MailboxConnection {
                Name = request.Name,
                EmailAddress = request.EmailAddress,
                Provider = request.Provider,
                DefaultQueueId = request.DefaultQueueId,
                ImapHost = request.ImapHost,
                ImapPort = request.ImapPort,
                ImapUseSsl = request.ImapUseSsl,
                ImapUsername = request.ImapUsername,
                EncryptedImapPassword = request.ImapPassword != null ? pii.Encrypt(request.ImapPassword) : null,
                SmtpHost = request.SmtpHost,
                SmtpPort = request.SmtpPort,
                SmtpUseSsl = request.SmtpUseSsl,
                SmtpUsername = request.SmtpUsername,
                EncryptedSmtpPassword = request.SmtpPassword != null ? pii.Encrypt(request.SmtpPassword) : null,
                GraphTenantId = request.GraphTenantId,
                GraphClientId = request.GraphClientId,
                EncryptedGraphClientSecret = request.GraphClientSecret != null ? pii.Encrypt(request.GraphClientSecret) : null,
                GraphMailboxUserId = request.GraphMailboxUserId,
                PollIntervalSeconds = request.PollIntervalSeconds,
                AutoCreateContacts = request.AutoCreateContacts
            };

            db.Set<MailboxConnection>().Add(connection);
            await db.SaveChangesAsync();

            return Results.Created($"/api/mailbox-connections/{connection.Id}", Result.Ok(MapToDto(connection)));
        }).RequirePermission("mailbox-connections.create").WithTags("Email");
    }

    internal static MailboxConnectionDto MapToDto(MailboxConnection c) => new() {
        Id = c.Id,
        Name = c.Name,
        EmailAddress = c.EmailAddress,
        Provider = c.Provider,
        Status = c.Status,
        DefaultQueueId = c.DefaultQueueId,
        ImapHost = c.ImapHost,
        ImapPort = c.ImapPort,
        ImapUseSsl = c.ImapUseSsl,
        SmtpHost = c.SmtpHost,
        SmtpPort = c.SmtpPort,
        SmtpUseSsl = c.SmtpUseSsl,
        GraphTenantId = c.GraphTenantId,
        GraphClientId = c.GraphClientId,
        GraphMailboxUserId = c.GraphMailboxUserId,
        PollIntervalSeconds = c.PollIntervalSeconds,
        AutoCreateContacts = c.AutoCreateContacts,
        LastPollAt = c.LastPollAt,
        MessageCount = c.MessageCount,
        LastError = c.LastError,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt
    };
}
