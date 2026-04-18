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

namespace OpenPsa.Modules.TimeEntries.Features.UpdateRateCard;

public class UpdateRateCardEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapPut("/api/rate-cards/{id:guid}", async (Guid id, UpdateRateCardRequest request, OpenPsaDbContext db, IMessageBus bus, CancellationToken ct) => {
            var rateCard = await db.Set<RateCard>().Include(r => r.Entries).FirstOrDefaultAsync(r => r.Id == id, ct);
            if (rateCard == null) return Results.NotFound();

            if (request.IsDefault && !rateCard.IsDefault) {
                var existing = await db.Set<RateCard>()
                    .Where(r => r.IsDefault && r.ClientId == rateCard.ClientId && r.Id != id)
                    .ToListAsync(ct);
                foreach (var r in existing) r.IsDefault = false;
            }

            rateCard.Name = request.Name;
            rateCard.IsDefault = request.IsDefault;

            db.Set<RateCardEntry>().RemoveRange(rateCard.Entries);
            rateCard.Entries = request.Entries.Select(e => new RateCardEntry {
                ServiceType = e.ServiceType,
                HourlyRate = e.HourlyRate,
                AfterHoursRate = e.AfterHoursRate
            }).ToList();

            await db.SaveChangesAsync(ct);

            string? clientName = null;
            if (rateCard.ClientId.HasValue)
                clientName = (await bus.InvokeAsync<GetClientNameResponse>(new GetClientNameQuery(rateCard.ClientId.Value), ct)).Name;

            return Results.Ok(Result.Ok(CreateRateCardEndpoint.MapToDto(rateCard, clientName)));
        }).RequirePermission("rate-cards.update").WithTags("Rate Cards");
    }
}
