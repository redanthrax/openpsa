using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Clients.Models;

namespace OpenPsa.Modules.Clients.Features.DeleteClient;

public class DeleteClientEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapDelete("/api/clients/{id:guid}", async (Guid id, OpenPsaDbContext db, CancellationToken ct) => {
            var client = await db.Set<Client>().FirstOrDefaultAsync(x => x.Id == id, ct);
            if (client == null) return Results.NotFound();

            db.Set<Client>().Remove(client);
            await db.SaveChangesAsync(ct);

            return Results.Ok(Result.Ok(true));
        }).RequirePermission("clients.delete").WithTags("Clients");
    }
}
