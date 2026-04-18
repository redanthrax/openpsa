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

namespace OpenPsa.Modules.Email.Features.GetAllMailboxConnections;

public class GetAllMailboxConnectionsEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapGet("/api/mailbox-connections", async (OpenPsaDbContext db) => {
            var connections = await db.Set<MailboxConnection>()
                .OrderBy(c => c.Name)
                .Select(c => new MailboxConnectionSummaryDto {
                    Id = c.Id,
                    Name = c.Name,
                    EmailAddress = c.EmailAddress,
                    Provider = c.Provider,
                    Status = c.Status,
                    LastPollAt = c.LastPollAt,
                    MessageCount = c.MessageCount,
                    LastError = c.LastError
                })
                .ToListAsync();

            return Results.Ok(Result.Ok(connections));
        }).RequirePermission("mailbox-connections.list").WithTags("Email");
    }
}
