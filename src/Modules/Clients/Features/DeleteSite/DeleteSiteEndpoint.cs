using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OpenPsa.Modules.Clients.Models;

namespace OpenPsa.Modules.Clients.Features.DeleteSite;

public class DeleteSiteEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapDelete("/api/sites/{id:guid}", async (Guid id, OpenPsaDbContext db, CancellationToken ct) => {
            var site = await db.Set<Site>().FindAsync([id], ct);
            if (site is null)
                return Results.Json(Result.Fail<object>("Site not found"), statusCode: 404);

            db.Set<Site>().Remove(site);
            await db.SaveChangesAsync(ct);
            return Results.Ok(Result.Ok<object?>(null));
        }).RequirePermission("sites.delete").WithTags("Sites");
    }
}
