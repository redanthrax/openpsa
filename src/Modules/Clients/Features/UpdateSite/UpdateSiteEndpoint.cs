using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Results;
using Contracts.Sites;
using IntegrationEvents.Clients;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OpenPsa.Modules.Clients.Models;
using Wolverine;

namespace OpenPsa.Modules.Clients.Features.UpdateSite;

public class UpdateSiteEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapPut("/api/sites/{id:guid}", async (
            Guid id, UpdateSiteRequest request, OpenPsaDbContext db, IMessageBus bus, CancellationToken ct) => {

            var site = await db.Set<Site>().FindAsync([id], ct);
            if (site is null)
                return Results.Json(Result.Fail<object>("Site not found"), statusCode: 404);

            site.Name = request.Name;
            site.Address = request.Address;
            site.City = request.City;
            site.State = request.State;
            site.PostalCode = request.PostalCode;
            site.Country = request.Country;
            site.Timezone = request.Timezone;
            site.Phone = request.Phone;
            site.Notes = request.Notes;
            site.IsPrimary = request.IsPrimary;
            site.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync(ct);

            var clientName = (await bus.InvokeAsync<GetClientNameResponse>(
                new GetClientNameQuery(site.ClientId), ct)).Name;

            return Results.Ok(Result.Ok(CreateSite.CreateSiteEndpoint.MapToDto(site, clientName)));
        }).RequirePermission("sites.update").WithTags("Sites");
    }
}
