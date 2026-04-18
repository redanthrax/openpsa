using Common.Authorization;
using Common.Database;
using Common.Modules;
using Common.Security;
using Contracts.Email;
using Contracts.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Email.Models;
using OpenPsa.Modules.Email.Services;
using IntegrationEvents.Email;
using Wolverine;

namespace OpenPsa.Modules.Email.Features.SendTicketEmail;

public class SendTicketEmailEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapPost("/api/email/send", async (
            SendTicketEmailRequest request, OpenPsaDbContext db, IPiiEncryptionService pii,
            GraphMailService graphMail, IMessageBus bus, CancellationToken ct) => {

            var mailbox = await db.Set<MailboxConnection>()
                .Where(m => m.Status == MailboxConnectionStatus.Active)
                .FirstOrDefaultAsync(ct);

            if (mailbox is null)
                return Results.Json(Result.Fail<EmailMessageDto>("No active mailbox connection configured"), statusCode: 400);

            var emailMsg = new EmailMessage {
                MailboxConnectionId = mailbox.Id,
                TicketId = request.TicketId,
                Direction = EmailDirection.Outbound,
                DeliveryStatus = EmailDeliveryStatus.Queued,
                FromAddress = mailbox.EmailAddress,
                FromName = mailbox.Name,
                ToAddress = request.ToAddress,
                Subject = request.Subject,
                BodyHtml = request.BodyHtml,
                SentAt = DateTime.UtcNow
            };

            string? inReplyTo = null;
            string? references = null;

            var lastInbound = await db.Set<EmailMessage>()
                .Where(e => e.TicketId == request.TicketId && e.Direction == EmailDirection.Inbound && e.MessageId != null)
                .OrderByDescending(e => e.SentAt)
                .Select(e => new { e.MessageId, e.References })
                .FirstOrDefaultAsync(ct);

            if (lastInbound != null) {
                inReplyTo = lastInbound.MessageId;
                references = string.IsNullOrEmpty(lastInbound.References)
                    ? lastInbound.MessageId
                    : $"{lastInbound.References} {lastInbound.MessageId}";
                emailMsg.InReplyTo = inReplyTo;
                emailMsg.References = references;
            }

            try {
                if (mailbox.Provider == MailboxProvider.Imap) {
                    await SendViaSmtpAsync(mailbox, request, pii, inReplyTo, references, ct);
                } else if (mailbox.Provider == MailboxProvider.MicrosoftGraph) {
                    await graphMail.SendAsync(mailbox, request, inReplyTo, references, ct);
                }

                emailMsg.DeliveryStatus = EmailDeliveryStatus.Sent;
            }
            catch (Exception ex) {
                emailMsg.DeliveryStatus = EmailDeliveryStatus.Failed;
                emailMsg.ErrorDetails = ex.Message;
            }

            db.Set<EmailMessage>().Add(emailMsg);
            mailbox.MessageCount++;
            await db.SaveChangesAsync(ct);

            if (emailMsg.DeliveryStatus == EmailDeliveryStatus.Sent)
                await bus.PublishAsync(new EmailSent(emailMsg.Id, emailMsg.TicketId, emailMsg.ToAddress, emailMsg.Subject));
            else
                await bus.PublishAsync(new EmailDeliveryFailed(emailMsg.Id, emailMsg.ErrorDetails ?? "Unknown error"));

            var dto = MapToDto(emailMsg);
            return emailMsg.DeliveryStatus == EmailDeliveryStatus.Sent
                ? Results.Ok(Result.Ok(dto))
                : Results.Json(Result.Fail<EmailMessageDto>($"Send failed: {emailMsg.ErrorDetails}"), statusCode: 500);
        }).RequirePermission("email.send").WithTags("Email");
    }

    private static async Task SendViaSmtpAsync(
        MailboxConnection mailbox, SendTicketEmailRequest request, IPiiEncryptionService pii,
        string? inReplyTo, string? references, CancellationToken ct) {
        if (string.IsNullOrEmpty(mailbox.SmtpHost))
            throw new InvalidOperationException("SMTP host not configured on mailbox connection");

        var password = mailbox.EncryptedSmtpPassword != null
            ? pii.Decrypt(mailbox.EncryptedSmtpPassword) : string.Empty;

        var message = new MimeKit.MimeMessage();
        message.From.Add(new MimeKit.MailboxAddress(mailbox.Name, mailbox.EmailAddress));
        message.To.Add(MimeKit.MailboxAddress.Parse(request.ToAddress));
        message.Subject = request.Subject;
        message.Body = new MimeKit.TextPart("html") { Text = request.BodyHtml };

        if (!string.IsNullOrEmpty(inReplyTo))
            message.InReplyTo = inReplyTo;
        if (!string.IsNullOrEmpty(references)) {
            foreach (var r in references.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                message.References.Add(r);
        }

        using var client = new MailKit.Net.Smtp.SmtpClient();
        await client.ConnectAsync(mailbox.SmtpHost, mailbox.SmtpPort ?? 587, mailbox.SmtpUseSsl, ct);
        await client.AuthenticateAsync(mailbox.SmtpUsername ?? mailbox.EmailAddress, password, ct);
        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);
    }

    internal static EmailMessageDto MapToDto(EmailMessage e) => new() {
        Id = e.Id,
        MailboxConnectionId = e.MailboxConnectionId,
        TicketId = e.TicketId,
        ContactId = e.ContactId,
        ClientId = e.ClientId,
        Direction = e.Direction,
        DeliveryStatus = e.DeliveryStatus,
        FromAddress = e.FromAddress,
        FromName = e.FromName,
        ToAddress = e.ToAddress,
        Subject = e.Subject,
        BodyHtml = e.BodyHtml,
        BodyText = e.BodyText,
        MessageId = e.MessageId,
        InReplyTo = e.InReplyTo,
        References = e.References,
        AttachmentCount = e.AttachmentCount,
        ErrorDetails = e.ErrorDetails,
        SentAt = e.SentAt,
        CreatedAt = e.CreatedAt
    };
}
