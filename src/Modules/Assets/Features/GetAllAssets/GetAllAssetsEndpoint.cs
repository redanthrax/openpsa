using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Assets;
using Contracts.Results;
using IntegrationEvents.Clients;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Assets.Models;
using Wolverine;

namespace OpenPsa.Modules.Assets.Features.GetAllAssets;

public class GetAllAssetsEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapGet("/api/assets", async (
            Guid? clientId, AssetType? type, AssetStatus? status,
            OpenPsaDbContext db, IMessageBus bus, CancellationToken ct) => {

            var query = db.Set<Asset>().AsQueryable();
            if (clientId.HasValue) query = query.Where(a => a.ClientId == clientId.Value);
            if (type.HasValue) query = query.Where(a => a.Type == type.Value);
            if (status.HasValue) query = query.Where(a => a.Status == status.Value);

            var assets = await query.OrderBy(a => a.Name).ToListAsync(ct);

            var clientIds = assets.Select(a => a.ClientId).Distinct().ToList();
            var clientNames = (await bus.InvokeAsync<GetClientNamesResponse>(
                new GetClientNamesQuery(clientIds), ct)).Names;

            var dtos = assets.Select(a => new AssetSummaryDto {
                Id = a.Id,
                Name = a.Name,
                Type = a.Type,
                Status = a.Status,
                ClientName = clientNames.GetValueOrDefault(a.ClientId),
                SerialNumber = a.SerialNumber,
                Manufacturer = a.Manufacturer,
                Model = a.Model,
                WarrantyExpiry = a.WarrantyExpiry
            }).ToList();

            return Results.Ok(Result.Ok(dtos));
        }).RequirePermission("assets.list").WithTags("Assets");
    }
}
