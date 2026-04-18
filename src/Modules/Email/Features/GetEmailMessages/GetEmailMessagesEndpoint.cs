using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Email;
using Contracts.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Email.Models;

namespace OpenPsa.Modules.Email.Features.GetEmailMessages;

public class GetEmailMessagesEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapGet("/api/email/messages", async (
            Guid? ticketId, Guid? clientId, Guid? mailboxConnectionId, OpenPsaDbContext db, CancellationToken ct) => {

            var query = db.Set<EmailMessage>().AsQueryable();

            if (ticketId.HasValue) query = query.Where(e => e.TicketId == ticketId.Value);
            if (clientId.HasValue) query = query.Where(e => e.ClientId == clientId.Value);
            if (mailboxConnectionId.HasValue) query = query.Where(e => e.MailboxConnectionId == mailboxConnectionId.Value);

            var messages = await query
                .OrderByDescending(e => e.SentAt)
                .Take(100)
                .Select(e => new EmailMessageSummaryDto {
                    Id = e.Id,
                    TicketId = e.TicketId,
                    Direction = e.Direction,
                    DeliveryStatus = e.DeliveryStatus,
                    FromAddress = e.FromAddress,
                    Subject = e.Subject,
                    AttachmentCount = e.AttachmentCount,
                    SentAt = e.SentAt
                })
                .ToListAsync(ct);

            return Results.Ok(Result.Ok(messages));
        }).RequirePermission("email.view-messages").WithTags("Email");

        app.MapGet("/api/email/messages/{id:guid}", async (Guid id, OpenPsaDbContext db, CancellationToken ct) => {
            var msg = await db.Set<EmailMessage>().FindAsync([id], ct);
            if (msg is null)
                return Results.Json(Result.Fail<EmailMessageDto>("Email message not found"), statusCode: 404);

            return Results.Ok(Result.Ok(SendTicketEmail.SendTicketEmailEndpoint.MapToDto(msg)));
        }).RequirePermission("email.view-messages").WithTags("Email");
    }
}
