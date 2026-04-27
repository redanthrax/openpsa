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

namespace OpenPsa.Modules.Assets.Features.CreateAsset;

public class CreateAssetEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapPost("/api/assets", async (
            CreateAssetRequest request, OpenPsaDbContext db, IMessageBus bus, CancellationToken ct) => {

                var clientResponse = await bus.InvokeAsync<GetClientNameResponse>(
                    new GetClientNameQuery(request.ClientId), ct);
                if (!clientResponse.Found)
                    return Results.Json(Result.Fail<AssetDto>("Client not found"), statusCode: 404);

                var asset = new Asset {
                    Name = request.Name,
                    Type = request.Type,
                    Status = request.Status,
                    ClientId = request.ClientId,
                    SiteId = request.SiteId,
                    SerialNumber = request.SerialNumber,
                    Manufacturer = request.Manufacturer,
                    Model = request.Model,
                    OperatingSystem = request.OperatingSystem,
                    IpAddress = request.IpAddress,
                    MacAddress = request.MacAddress,
                    PurchaseDate = request.PurchaseDate,
                    WarrantyExpiry = request.WarrantyExpiry,
                    PurchasePrice = request.PurchasePrice,
                    Location = request.Location,
                    Notes = request.Notes
                };

                db.Set<Asset>().Add(asset);
                await db.SaveChangesAsync(ct);

                return Results.Created($"/api/assets/{asset.Id}", Result.Ok(MapToDto(asset, clientResponse.Name)));
            }).RequirePermission("assets.create").WithTags("Assets");
    }

    internal static AssetDto MapToDto(Asset a, string? clientName = null) => new() {
        Id = a.Id,
        Name = a.Name,
        Type = a.Type,
        Status = a.Status,
        ClientId = a.ClientId,
        ClientName = clientName,
        SiteId = a.SiteId,
        SerialNumber = a.SerialNumber,
        Manufacturer = a.Manufacturer,
        Model = a.Model,
        OperatingSystem = a.OperatingSystem,
        IpAddress = a.IpAddress,
        MacAddress = a.MacAddress,
        PurchaseDate = a.PurchaseDate,
        WarrantyExpiry = a.WarrantyExpiry,
        PurchasePrice = a.PurchasePrice,
        Location = a.Location,
        Notes = a.Notes,
        CreatedAt = a.CreatedAt,
        UpdatedAt = a.UpdatedAt
    };
}
