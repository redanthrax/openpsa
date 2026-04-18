using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Results;
using IntegrationEvents.Clients;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OpenPsa.Modules.Assets.Models;
using Wolverine;

namespace OpenPsa.Modules.Assets.Features.GetAssetById;

public class GetAssetByIdEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapGet("/api/assets/{id:guid}", async (
            Guid id, OpenPsaDbContext db, IMessageBus bus, CancellationToken ct) => {

            var asset = await db.Set<Asset>().FindAsync([id], ct);
            if (asset is null)
                return Results.Json(Result.Fail<object>("Asset not found"), statusCode: 404);

            var clientName = (await bus.InvokeAsync<GetClientNameResponse>(
                new GetClientNameQuery(asset.ClientId), ct)).Name;

            return Results.Ok(Result.Ok(CreateAsset.CreateAssetEndpoint.MapToDto(asset, clientName)));
        }).RequirePermission("assets.view").WithTags("Assets");
    }
}
