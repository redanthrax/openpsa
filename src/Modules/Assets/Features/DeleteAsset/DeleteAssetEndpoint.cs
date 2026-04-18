using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OpenPsa.Modules.Assets.Models;

namespace OpenPsa.Modules.Assets.Features.DeleteAsset;

public class DeleteAssetEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapDelete("/api/assets/{id:guid}", async (Guid id, OpenPsaDbContext db, CancellationToken ct) => {
            var asset = await db.Set<Asset>().FindAsync([id], ct);
            if (asset is null)
                return Results.Json(Result.Fail<object>("Asset not found"), statusCode: 404);

            db.Set<Asset>().Remove(asset);
            await db.SaveChangesAsync(ct);
            return Results.Ok(Result.Ok<object?>(null));
        }).RequirePermission("assets.delete").WithTags("Assets");
    }
}
