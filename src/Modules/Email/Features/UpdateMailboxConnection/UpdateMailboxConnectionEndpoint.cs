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

namespace OpenPsa.Modules.Email.Features.UpdateMailboxConnection;

public class UpdateMailboxConnectionEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapPut("/api/mailbox-connections/{id:guid}", async (
            Guid id, UpdateMailboxConnectionRequest request, OpenPsaDbContext db, IPiiEncryptionService pii) => {

                var connection = await db.Set<MailboxConnection>().FindAsync(id);
                if (connection is null)
                    return Results.Json(Result.Fail<object>("Mailbox connection not found"), statusCode: 404);

                connection.Name = request.Name;
                connection.EmailAddress = request.EmailAddress;
                connection.Status = request.Status;
                connection.DefaultQueueId = request.DefaultQueueId;
                connection.ImapHost = request.ImapHost;
                connection.ImapPort = request.ImapPort;
                connection.ImapUseSsl = request.ImapUseSsl;
                connection.ImapUsername = request.ImapUsername;
                if (request.ImapPassword != null)
                    connection.EncryptedImapPassword = pii.Encrypt(request.ImapPassword);
                connection.SmtpHost = request.SmtpHost;
                connection.SmtpPort = request.SmtpPort;
                connection.SmtpUseSsl = request.SmtpUseSsl;
                connection.SmtpUsername = request.SmtpUsername;
                if (request.SmtpPassword != null)
                    connection.EncryptedSmtpPassword = pii.Encrypt(request.SmtpPassword);
                connection.GraphTenantId = request.GraphTenantId;
                connection.GraphClientId = request.GraphClientId;
                if (request.GraphClientSecret != null)
                    connection.EncryptedGraphClientSecret = pii.Encrypt(request.GraphClientSecret);
                connection.GraphMailboxUserId = request.GraphMailboxUserId;
                connection.PollIntervalSeconds = request.PollIntervalSeconds;
                connection.AutoCreateContacts = request.AutoCreateContacts;
                connection.UpdatedAt = DateTime.UtcNow;

                await db.SaveChangesAsync();
                return Results.Ok(Result.Ok(CreateMailboxConnection.CreateMailboxConnectionEndpoint.MapToDto(connection)));
            }).RequirePermission("mailbox-connections.update").WithTags("Email");
    }
}
