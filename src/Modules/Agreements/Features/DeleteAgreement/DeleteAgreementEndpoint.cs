using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Agreements.Models;

namespace OpenPsa.Modules.Agreements.Features.DeleteAgreement;

public class DeleteAgreementEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapDelete("/api/agreements/{id:guid}", async (Guid id, OpenPsaDbContext db, CancellationToken ct) => {
            var agreement = await db.Set<Agreement>().FirstOrDefaultAsync(a => a.Id == id, ct);
            if (agreement == null) return Results.NotFound();

            db.Set<Agreement>().Remove(agreement);
            await db.SaveChangesAsync(ct);

            return Results.Ok(Result.Ok(true));
        }).RequirePermission("agreements.delete").WithTags("Agreements");
    }
}
