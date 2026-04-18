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

namespace OpenPsa.Modules.TimeEntries.Features.CreateRateCard;

public class CreateRateCardEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapPost("/api/rate-cards", async (CreateRateCardRequest request, OpenPsaDbContext db, IMessageBus bus, CancellationToken ct) => {
            string? clientName = null;
            if (request.ClientId.HasValue) {
                var clientResponse = await bus.InvokeAsync<GetClientNameResponse>(new GetClientNameQuery(request.ClientId.Value), ct);
                if (!clientResponse.Found)
                    return Results.Json(Result.Fail<RateCardDto>("Client not found"), statusCode: 404);
                clientName = clientResponse.Name;
            }

            if (request.IsDefault) {
                var existing = await db.Set<RateCard>()
                    .Where(r => r.IsDefault && r.ClientId == request.ClientId)
                    .ToListAsync(ct);
                foreach (var r in existing) r.IsDefault = false;
            }

            var rateCard = new RateCard {
                Name = request.Name,
                ClientId = request.ClientId,
                IsDefault = request.IsDefault,
                Entries = request.Entries.Select(e => new RateCardEntry {
                    ServiceType = e.ServiceType,
                    HourlyRate = e.HourlyRate,
                    AfterHoursRate = e.AfterHoursRate
                }).ToList()
            };

            db.Set<RateCard>().Add(rateCard);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/rate-cards/{rateCard.Id}", Result.Ok(MapToDto(rateCard, clientName)));
        }).RequirePermission("rate-cards.create").WithTags("Rate Cards");
    }

    internal static RateCardDto MapToDto(RateCard rateCard, string? clientName) => new() {
        Id = rateCard.Id,
        Name = rateCard.Name,
        ClientId = rateCard.ClientId,
        ClientName = clientName,
        IsDefault = rateCard.IsDefault,
        Entries = rateCard.Entries.Select(e => new RateCardEntryDto {
            Id = e.Id,
            ServiceType = e.ServiceType,
            HourlyRate = e.HourlyRate,
            AfterHoursRate = e.AfterHoursRate
        }).ToList(),
        CreatedAt = rateCard.CreatedAt,
        UpdatedAt = rateCard.UpdatedAt
    };
}
