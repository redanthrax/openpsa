using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Assets;
using Contracts.Results;
using IntegrationEvents.Clients;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OpenPsa.Modules.Assets.Models;
using Wolverine;

namespace OpenPsa.Modules.Assets.Features.UpdateAsset;

public class UpdateAssetEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapPut("/api/assets/{id:guid}", async (
            Guid id, UpdateAssetRequest request, OpenPsaDbContext db, IMessageBus bus, CancellationToken ct) => {

            var asset = await db.Set<Asset>().FindAsync([id], ct);
            if (asset is null)
                return Results.Json(Result.Fail<object>("Asset not found"), statusCode: 404);

            asset.Name = request.Name;
            asset.Type = request.Type;
            asset.Status = request.Status;
            asset.SiteId = request.SiteId;
            asset.SerialNumber = request.SerialNumber;
            asset.Manufacturer = request.Manufacturer;
            asset.Model = request.Model;
            asset.OperatingSystem = request.OperatingSystem;
            asset.IpAddress = request.IpAddress;
            asset.MacAddress = request.MacAddress;
            asset.PurchaseDate = request.PurchaseDate;
            asset.WarrantyExpiry = request.WarrantyExpiry;
            asset.PurchasePrice = request.PurchasePrice;
            asset.Location = request.Location;
            asset.Notes = request.Notes;
            asset.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync(ct);

            var clientName = (await bus.InvokeAsync<GetClientNameResponse>(
                new GetClientNameQuery(asset.ClientId), ct)).Name;

            return Results.Ok(Result.Ok(CreateAsset.CreateAssetEndpoint.MapToDto(asset, clientName)));
        }).RequirePermission("assets.update").WithTags("Assets");
    }
}
