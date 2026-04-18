using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Email.Models;

namespace OpenPsa.Modules.Email.Features.GetMailboxConnectionById;

public class GetMailboxConnectionByIdEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapGet("/api/mailbox-connections/{id:guid}", async (Guid id, OpenPsaDbContext db) => {
            var connection = await db.Set<MailboxConnection>().FindAsync(id);
            if (connection is null)
                return Results.Json(Result.Fail<object>("Mailbox connection not found"), statusCode: 404);

            return Results.Ok(Result.Ok(CreateMailboxConnection.CreateMailboxConnectionEndpoint.MapToDto(connection)));
        }).RequirePermission("mailbox-connections.view").WithTags("Email");
    }
}
