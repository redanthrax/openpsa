using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Agreements;
using Contracts.Results;
using IntegrationEvents.Clients;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Agreements.Models;
using Wolverine;

namespace OpenPsa.Modules.Agreements.Features.CreateAgreement;

public class CreateAgreementEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapPost("/api/agreements", async (CreateAgreementRequest request, OpenPsaDbContext db, IMessageBus bus, CancellationToken ct) => {
            var clientResponse = await bus.InvokeAsync<GetClientNameResponse>(new GetClientNameQuery(request.ClientId), ct);
            if (!clientResponse.Found)
                return Results.Json(Result.Fail<AgreementDto>("Client not found"), statusCode: 404);

            var agreement = new Agreement {
                Name = request.Name,
                Description = request.Description,
                Type = request.Type,
                ClientId = request.ClientId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                MonthlyAmount = request.MonthlyAmount,
                TotalValue = request.TotalValue,
                BlockHoursTotal = request.BlockHoursTotal,
                BlockHoursUsed = 0,
                HourlyRate = request.HourlyRate,
                RenewalNoticeDays = request.RenewalNoticeDays,
                SlaPolicyId = request.SlaPolicyId,
                Status = AgreementStatus.Draft
            };

            db.Set<Agreement>().Add(agreement);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/agreements/{agreement.Id}", Result.Ok(MapToDto(agreement, clientResponse.Name ?? string.Empty)));
        }).RequirePermission("agreements.create").WithTags("Agreements");
    }

    internal static AgreementDto MapToDto(Agreement a, string clientName) => new() {
        Id = a.Id,
        Name = a.Name,
        Description = a.Description,
        Type = a.Type,
        Status = a.Status,
        ClientId = a.ClientId,
        ClientName = clientName,
        StartDate = a.StartDate,
        EndDate = a.EndDate,
        MonthlyAmount = a.MonthlyAmount,
        TotalValue = a.TotalValue,
        BlockHoursTotal = a.BlockHoursTotal,
        BlockHoursUsed = a.BlockHoursUsed,
        HourlyRate = a.HourlyRate,
        RenewalNoticeDays = a.RenewalNoticeDays,
        SlaPolicyId = a.SlaPolicyId,
        CreatedAt = a.CreatedAt,
        UpdatedAt = a.UpdatedAt
    };
}
