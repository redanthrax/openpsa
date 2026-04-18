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
using OpenPsa.Modules.TimeEntries.Models;
using Wolverine;

namespace OpenPsa.Modules.TimeEntries.Features.GetAllRateCards;

public class GetAllRateCardsEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapGet("/api/rate-cards", async (OpenPsaDbContext db, IMessageBus bus, Guid? clientId, CancellationToken ct) => {
            var query = db.Set<RateCard>().Include(r => r.Entries).AsQueryable();
            if (clientId.HasValue) query = query.Where(r => r.ClientId == clientId.Value || r.ClientId == null);

            var rateCards = await query.OrderByDescending(r => r.CreatedAt).ToListAsync(ct);

            var clientIds = rateCards.Where(r => r.ClientId.HasValue).Select(r => r.ClientId!.Value).Distinct().ToList();
            var clientNames = clientIds.Count > 0
                ? (await bus.InvokeAsync<GetClientNamesResponse>(new GetClientNamesQuery(clientIds), ct)).Names
                : new Dictionary<Guid, string>();

            var dtos = rateCards.Select(r => new RateCardDto {
                Id = r.Id,
                Name = r.Name,
                ClientId = r.ClientId,
                ClientName = r.ClientId.HasValue ? clientNames.GetValueOrDefault(r.ClientId.Value) : null,
                IsDefault = r.IsDefault,
                Entries = r.Entries.Select(e => new RateCardEntryDto {
                    Id = e.Id,
                    ServiceType = e.ServiceType,
                    HourlyRate = e.HourlyRate,
                    AfterHoursRate = e.AfterHoursRate
                }).ToList(),
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            });

            return Results.Ok(Result.Ok(dtos));
        }).RequirePermission("rate-cards.list").WithTags("Rate Cards");
    }
}
