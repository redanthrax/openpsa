using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.TimeEntries.Models;

namespace OpenPsa.Modules.TimeEntries.Features.DeleteRateCard;

public class DeleteRateCardEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapDelete("/api/rate-cards/{id:guid}", async (Guid id, OpenPsaDbContext db, CancellationToken ct) => {
            var rateCard = await db.Set<RateCard>().Include(r => r.Entries).FirstOrDefaultAsync(r => r.Id == id, ct);
            if (rateCard == null) return Results.NotFound();

            db.Set<RateCard>().Remove(rateCard);
            await db.SaveChangesAsync(ct);

            return Results.Ok(Result.Ok(true));
        }).RequirePermission("rate-cards.delete").WithTags("Rate Cards");
    }
}
