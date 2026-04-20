using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Results;
using Contracts.Sla;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Sla.Models;

namespace OpenPsa.Modules.Sla.Features.UpdateBusinessHoursCalendar;

public class UpdateBusinessHoursCalendarEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapPut("/api/business-hours/{id:guid}", async (Guid id, UpdateBusinessHoursCalendarRequest request, OpenPsaDbContext db, CancellationToken ct) => {
            var calendar = await db.Set<BusinessHoursCalendar>()
                .Include(c => c.Schedules)
                .Include(c => c.Holidays)
                .FirstOrDefaultAsync(c => c.Id == id, ct);

            if (calendar is null) return Results.NotFound();

            if (request.IsDefault && !calendar.IsDefault) {
                var existing = await db.Set<BusinessHoursCalendar>().Where(c => c.IsDefault && c.Id != id).ToListAsync(ct);
                foreach (var c in existing) c.IsDefault = false;
            }

            calendar.Name = request.Name;
            calendar.TimeZoneId = request.TimeZoneId;
            calendar.IsDefault = request.IsDefault;

            db.Set<BusinessHoursSchedule>().RemoveRange(calendar.Schedules);
            db.Set<BusinessHoursHoliday>().RemoveRange(calendar.Holidays);

            calendar.Schedules = request.Schedules.Select(s => new BusinessHoursSchedule {
                CalendarId = id,
                DayOfWeek = s.DayOfWeek,
                StartTime = s.StartTime,
                EndTime = s.EndTime
            }).ToList();

            calendar.Holidays = request.Holidays.Select(h => new BusinessHoursHoliday {
                CalendarId = id,
                Name = h.Name,
                Date = h.Date
            }).ToList();

            await db.SaveChangesAsync(ct);

            return Results.Ok(Result.Ok(GetAllBusinessHoursCalendars.GetAllBusinessHoursCalendarsEndpoint.MapToDto(calendar)));
        }).RequirePermission("sla-policies.update").WithTags("Business Hours");
    }
}
