using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Results;
using IntegrationEvents.Clients;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OpenPsa.Modules.Clients.Models;
using Wolverine;

namespace OpenPsa.Modules.Clients.Features.GetSiteById;

public class GetSiteByIdEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapGet("/api/sites/{id:guid}", async (
            Guid id, OpenPsaDbContext db, IMessageBus bus, CancellationToken ct) => {

            var site = await db.Set<Site>().FindAsync([id], ct);
            if (site is null)
                return Results.Json(Result.Fail<object>("Site not found"), statusCode: 404);

            var clientName = (await bus.InvokeAsync<GetClientNameResponse>(
                new GetClientNameQuery(site.ClientId), ct)).Name;

            return Results.Ok(Result.Ok(CreateSite.CreateSiteEndpoint.MapToDto(site, clientName)));
        }).RequirePermission("sites.view").WithTags("Sites");
    }
}
