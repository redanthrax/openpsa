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

namespace OpenPsa.Modules.Agreements.Features.GetAgreementById;

public class GetAgreementByIdEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapGet("/api/agreements/{id:guid}", async (Guid id, OpenPsaDbContext db, IMessageBus bus, CancellationToken ct) => {
            var agreement = await db.Set<Agreement>().FirstOrDefaultAsync(a => a.Id == id, ct);
            if (agreement == null) return Results.NotFound();

            var clientName = (await bus.InvokeAsync<GetClientNameResponse>(new GetClientNameQuery(agreement.ClientId), ct)).Name ?? string.Empty;

            return Results.Ok(Result.Ok(CreateAgreementEndpoint.MapToDto(agreement, clientName)));
        }).RequirePermission("agreements.view").WithTags("Agreements");
    }
}
