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

namespace OpenPsa.Modules.Sla.Features.CreateBusinessHoursCalendar;

public class CreateBusinessHoursCalendarEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapPost("/api/business-hours", async (CreateBusinessHoursCalendarRequest request, OpenPsaDbContext db, CancellationToken ct) => {
            if (request.IsDefault) {
                var existing = await db.Set<BusinessHoursCalendar>().Where(c => c.IsDefault).ToListAsync(ct);
                foreach (var c in existing) c.IsDefault = false;
            }

            var calendar = new BusinessHoursCalendar {
                Name = request.Name,
                TimeZoneId = request.TimeZoneId,
                IsDefault = request.IsDefault,
                Schedules = request.Schedules.Select(s => new BusinessHoursSchedule {
                    DayOfWeek = s.DayOfWeek,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime
                }).ToList(),
                Holidays = request.Holidays.Select(h => new BusinessHoursHoliday {
                    Name = h.Name,
                    Date = h.Date
                }).ToList()
            };

            db.Set<BusinessHoursCalendar>().Add(calendar);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/business-hours/{calendar.Id}",
                Result.Ok(GetAllBusinessHoursCalendars.GetAllBusinessHoursCalendarsEndpoint.MapToDto(calendar)));
        }).RequirePermission("sla-policies.create").WithTags("Business Hours");
    }
}
