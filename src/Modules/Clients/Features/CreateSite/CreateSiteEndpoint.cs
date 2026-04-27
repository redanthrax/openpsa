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

namespace OpenPsa.Modules.Clients.Features.CreateSite;

public class CreateSiteEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapPost("/api/sites", async (
            CreateSiteRequest request, OpenPsaDbContext db, IMessageBus bus, CancellationToken ct) => {

                var clientResponse = await bus.InvokeAsync<GetClientNameResponse>(
                    new GetClientNameQuery(request.ClientId), ct);
                if (!clientResponse.Found)
                    return Results.Json(Result.Fail<SiteDto>("Client not found"), statusCode: 404);

                var site = new Site {
                    ClientId = request.ClientId,
                    Name = request.Name,
                    Address = request.Address,
                    City = request.City,
                    State = request.State,
                    PostalCode = request.PostalCode,
                    Country = request.Country,
                    Timezone = request.Timezone,
                    Phone = request.Phone,
                    Notes = request.Notes,
                    IsPrimary = request.IsPrimary
                };

                db.Set<Site>().Add(site);
                await db.SaveChangesAsync(ct);

                return Results.Created($"/api/sites/{site.Id}", Result.Ok(MapToDto(site, clientResponse.Name)));
            }).RequirePermission("sites.create").WithTags("Sites");
    }

    internal static SiteDto MapToDto(Site s, string? clientName = null) => new() {
        Id = s.Id,
        ClientId = s.ClientId,
        ClientName = clientName,
        Name = s.Name,
        Address = s.Address,
        City = s.City,
        State = s.State,
        PostalCode = s.PostalCode,
        Country = s.Country,
        Timezone = s.Timezone,
        Phone = s.Phone,
        Notes = s.Notes,
        IsPrimary = s.IsPrimary,
        CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt
    };
}
