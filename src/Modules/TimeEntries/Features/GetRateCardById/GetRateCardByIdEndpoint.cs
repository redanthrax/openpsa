using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Results;
using Contracts.TimeEntries;
using IntegrationEvents.Clients;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.TimeEntries.Features.CreateRateCard;
using OpenPsa.Modules.TimeEntries.Models;
using Wolverine;

namespace OpenPsa.Modules.TimeEntries.Features.GetRateCardById;

public class GetRateCardByIdEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapGet("/api/rate-cards/{id:guid}", async (Guid id, OpenPsaDbContext db, IMessageBus bus, CancellationToken ct) => {
            var rateCard = await db.Set<RateCard>().Include(r => r.Entries).FirstOrDefaultAsync(r => r.Id == id, ct);
            if (rateCard == null) return Results.NotFound();

            string? clientName = null;
            if (rateCard.ClientId.HasValue)
                clientName = (await bus.InvokeAsync<GetClientNameResponse>(new GetClientNameQuery(rateCard.ClientId.Value), ct)).Name;

            return Results.Ok(Result.Ok(CreateRateCardEndpoint.MapToDto(rateCard, clientName)));
        }).RequirePermission("rate-cards.view").WithTags("Rate Cards");
    }
}
