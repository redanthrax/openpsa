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
using OpenPsa.Modules.Email.Services;

namespace OpenPsa.Modules.Email.Features.TestMailboxConnection;

public class TestMailboxConnectionEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapPost("/api/mailbox-connections/{id:guid}/test", async (
            Guid id, OpenPsaDbContext db, IPiiEncryptionService pii, GraphMailService graphMail, CancellationToken ct) => {

            var connection = await db.Set<MailboxConnection>().FindAsync([id], ct);
            if (connection is null)
                return Results.Json(Result.Fail<TestMailboxConnectionResult>("Mailbox connection not found"), statusCode: 404);

            var result = new TestMailboxConnectionResult();

            try {
                if (connection.Provider == MailboxProvider.Imap) {
                    result = await TestImapAsync(connection, pii, ct);
                } else {
                    result = await graphMail.TestConnectionAsync(connection, ct);
                }
            }
            catch (Exception ex) {
                result.Success = false;
                result.Message = $"Connection failed: {ex.Message}";
            }

            return Results.Ok(Result.Ok(result));
        }).RequirePermission("mailbox-connections.update").WithTags("Email");
    }

    private static async Task<TestMailboxConnectionResult> TestImapAsync(
        MailboxConnection connection, IPiiEncryptionService pii, CancellationToken ct) {
        if (string.IsNullOrEmpty(connection.ImapHost))
            return new TestMailboxConnectionResult { Success = false, Message = "IMAP host not configured" };

        var password = connection.EncryptedImapPassword != null
            ? pii.Decrypt(connection.EncryptedImapPassword) : string.Empty;

        using var client = new MailKit.Net.Imap.ImapClient();
        await client.ConnectAsync(connection.ImapHost, connection.ImapPort ?? 993, connection.ImapUseSsl, ct);
        await client.AuthenticateAsync(connection.ImapUsername ?? connection.EmailAddress, password, ct);

        var inbox = client.Inbox;
        await inbox.OpenAsync(MailKit.FolderAccess.ReadOnly, ct);
        var count = inbox.Count;

        await client.DisconnectAsync(true, ct);

        return new TestMailboxConnectionResult {
            Success = true,
            Message = "Connection successful",
            MailboxInfo = $"INBOX: {count} messages"
        };
    }
}
