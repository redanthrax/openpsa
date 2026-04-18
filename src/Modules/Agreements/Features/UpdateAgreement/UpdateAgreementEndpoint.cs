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
using OpenPsa.Modules.Agreements.Features.CreateAgreement;
using OpenPsa.Modules.Agreements.Models;
using Wolverine;

namespace OpenPsa.Modules.Agreements.Features.UpdateAgreement;

public class UpdateAgreementEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapPut("/api/agreements/{id:guid}", async (Guid id, UpdateAgreementRequest request, OpenPsaDbContext db, IMessageBus bus, CancellationToken ct) => {
            var agreement = await db.Set<Agreement>().FirstOrDefaultAsync(a => a.Id == id, ct);
            if (agreement == null) return Results.NotFound();

            agreement.Name = request.Name;
            agreement.Description = request.Description;
            agreement.Type = request.Type;
            agreement.Status = request.Status;
            agreement.StartDate = request.StartDate;
            agreement.EndDate = request.EndDate;
            agreement.MonthlyAmount = request.MonthlyAmount;
            agreement.TotalValue = request.TotalValue;
            agreement.BlockHoursTotal = request.BlockHoursTotal;
            agreement.HourlyRate = request.HourlyRate;
            agreement.RenewalNoticeDays = request.RenewalNoticeDays;
            agreement.SlaPolicyId = request.SlaPolicyId;

            await db.SaveChangesAsync(ct);

            var clientName = (await bus.InvokeAsync<GetClientNameResponse>(new GetClientNameQuery(agreement.ClientId), ct)).Name ?? string.Empty;

            return Results.Ok(Result.Ok(CreateAgreementEndpoint.MapToDto(agreement, clientName)));
        }).RequirePermission("agreements.update").WithTags("Agreements");
    }
}
