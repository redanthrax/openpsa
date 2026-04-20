using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Sla.Models;

namespace OpenPsa.Modules.Sla.Features.GetBusinessHoursCalendarById;

public class GetBusinessHoursCalendarByIdEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapGet("/api/business-hours/{id:guid}", async (Guid id, OpenPsaDbContext db, CancellationToken ct) => {
            var calendar = await db.Set<BusinessHoursCalendar>()
                .Include(c => c.Schedules)
                .Include(c => c.Holidays)
                .FirstOrDefaultAsync(c => c.Id == id, ct);

            if (calendar is null) return Results.NotFound();

            return Results.Ok(Result.Ok(GetAllBusinessHoursCalendars.GetAllBusinessHoursCalendarsEndpoint.MapToDto(calendar)));
        }).RequirePermission("sla-policies.view").WithTags("Business Hours");
    }
}
