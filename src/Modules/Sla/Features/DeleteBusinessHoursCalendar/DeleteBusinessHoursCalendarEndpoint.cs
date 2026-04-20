using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Sla.Models;

namespace OpenPsa.Modules.Sla.Features.DeleteBusinessHoursCalendar;

public class DeleteBusinessHoursCalendarEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapDelete("/api/business-hours/{id:guid}", async (Guid id, OpenPsaDbContext db, CancellationToken ct) => {
            var calendar = await db.Set<BusinessHoursCalendar>().FirstOrDefaultAsync(c => c.Id == id, ct);
            if (calendar is null) return Results.NotFound();

            var inUse = await db.Set<SlaPolicy>().AnyAsync(p => p.BusinessHoursCalendarId == id, ct);
            if (inUse)
                return Results.Json(Result.Fail<object>("Cannot delete calendar that is assigned to an SLA policy."), statusCode: 409);

            db.Set<BusinessHoursCalendar>().Remove(calendar);
            await db.SaveChangesAsync(ct);

            return Results.Ok(Result.Ok<object>(null!));
        }).RequirePermission("sla-policies.delete").WithTags("Business Hours");
    }
}
