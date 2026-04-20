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

namespace OpenPsa.Modules.Sla.Features.GetAllBusinessHoursCalendars;

public class GetAllBusinessHoursCalendarsEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapGet("/api/business-hours", async (
            OpenPsaDbContext db,
            int page = 1, int pageSize = 25,
            CancellationToken ct = default) => {

            var totalCount = await db.Set<BusinessHoursCalendar>().CountAsync(ct);
            var calendars = await db.Set<BusinessHoursCalendar>()
                .Include(c => c.Schedules)
                .Include(c => c.Holidays)
                .OrderBy(c => c.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            var dtos = calendars.Select(MapToDto).ToList();
            return Results.Ok(PagedResult.Ok<BusinessHoursCalendarDto>(dtos, totalCount, page, pageSize));
        }).RequirePermission("sla-policies.list").WithTags("Business Hours");
    }

    internal static BusinessHoursCalendarDto MapToDto(BusinessHoursCalendar c) => new() {
        Id = c.Id,
        Name = c.Name,
        TimeZoneId = c.TimeZoneId,
        IsDefault = c.IsDefault,
        CreatedAt = c.CreatedAt,
        Schedules = c.Schedules.OrderBy(s => s.DayOfWeek).ThenBy(s => s.StartTime).Select(s => new BusinessHoursScheduleDto {
            Id = s.Id,
            DayOfWeek = s.DayOfWeek,
            StartTime = s.StartTime,
            EndTime = s.EndTime
        }).ToList(),
        Holidays = c.Holidays.OrderBy(h => h.Date).Select(h => new BusinessHoursHolidayDto {
            Id = h.Id,
            Name = h.Name,
            Date = h.Date
        }).ToList()
    };
}
