using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OpenPsa.Modules.Email.Models;

namespace OpenPsa.Modules.Email.Features.DeleteMailboxConnection;

public class DeleteMailboxConnectionEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapDelete("/api/mailbox-connections/{id:guid}", async (Guid id, OpenPsaDbContext db) => {
            var connection = await db.Set<MailboxConnection>().FindAsync(id);
            if (connection is null)
                return Results.Json(Result.Fail<object>("Mailbox connection not found"), statusCode: 404);

            db.Set<MailboxConnection>().Remove(connection);
            await db.SaveChangesAsync();
            return Results.Ok(Result.Ok<object?>(null));
        }).RequirePermission("mailbox-connections.delete").WithTags("Email");
    }
}
