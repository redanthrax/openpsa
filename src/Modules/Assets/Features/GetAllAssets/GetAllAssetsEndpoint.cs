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
            OpenPsaDbContext db, IMessageBus bus,
            Guid? clientId, AssetType? type, AssetStatus? status,
            int page = 1, int pageSize = 25,
            CancellationToken ct = default) => {

                var query = db.Set<Asset>().AsQueryable();
                if (clientId.HasValue) query = query.Where(a => a.ClientId == clientId.Value);
                if (type.HasValue) query = query.Where(a => a.Type == type.Value);
                if (status.HasValue) query = query.Where(a => a.Status == status.Value);

                var ordered = query.OrderBy(a => a.Name);
                var totalCount = await ordered.CountAsync(ct);
                var assets = await ordered
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(ct);

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

                return Results.Ok(PagedResult.Ok<AssetSummaryDto>(dtos, totalCount, page, pageSize));
            }).RequirePermission("assets.list").WithTags("Assets");
    }
}
