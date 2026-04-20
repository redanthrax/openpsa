using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Results;
using Contracts.Sites;
using IntegrationEvents.Clients;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Clients.Models;
using Wolverine;

namespace OpenPsa.Modules.Clients.Features.GetAllSites;

public class GetAllSitesEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapGet("/api/sites", async (
            Guid? clientId,
            int page = 1, int pageSize = 25,
            OpenPsaDbContext db = default!, IMessageBus bus = default!,
            CancellationToken ct = default) => {

            var query = db.Set<Site>().AsQueryable();
            if (clientId.HasValue) query = query.Where(s => s.ClientId == clientId.Value);

            var ordered = query.OrderBy(s => s.Name);
            var totalCount = await ordered.CountAsync(ct);
            var sites = await ordered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            var clientIds = sites.Select(s => s.ClientId).Distinct().ToList();
            var clientNames = (await bus.InvokeAsync<GetClientNamesResponse>(
                new GetClientNamesQuery(clientIds), ct)).Names;

            var dtos = sites.Select(s => new SiteSummaryDto {
                Id = s.Id,
                ClientId = s.ClientId,
                ClientName = clientNames.GetValueOrDefault(s.ClientId),
                Name = s.Name,
                City = s.City,
                State = s.State,
                Timezone = s.Timezone,
                IsPrimary = s.IsPrimary
            }).ToList();

            return Results.Ok(PagedResult.Ok<SiteSummaryDto>(dtos, totalCount, page, pageSize));
        }).RequirePermission("sites.list").WithTags("Sites");
    }
}
