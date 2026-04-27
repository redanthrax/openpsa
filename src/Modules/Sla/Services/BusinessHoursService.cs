using OpenPsa.Modules.Sla.Models;

namespace OpenPsa.Modules.Sla.Services;

public class BusinessHoursService {
    public static DateTime CalculateDeadline(DateTime startUtc, int slaMinutes, BusinessHoursCalendar? calendar) {
        if (calendar is null || calendar.Schedules.Count == 0)
            return startUtc.AddMinutes(slaMinutes);

        var tz = TimeZoneInfo.FindSystemTimeZoneById(calendar.TimeZoneId);
        var local = TimeZoneInfo.ConvertTimeFromUtc(startUtc, tz);
        var remaining = slaMinutes;
        var holidays = calendar.Holidays.Select(h => h.Date).ToHashSet();

        var maxIterations = slaMinutes + (slaMinutes / 60 * 24 * 7) + 400;
        var iterations = 0;

        while (remaining > 0 && iterations++ < maxIterations) {
            var today = DateOnly.FromDateTime(local);

            if (holidays.Contains(today)) {
                local = local.Date.AddDays(1);
                continue;
            }

            var schedules = calendar.Schedules
                .Where(s => s.DayOfWeek == local.DayOfWeek)
                .OrderBy(s => s.StartTime)
                .ToList();

            if (schedules.Count == 0) {
                local = local.Date.AddDays(1);
                continue;
            }

            var currentTime = TimeOnly.FromDateTime(local);

            foreach (var schedule in schedules) {
                if (currentTime >= schedule.EndTime) continue;

                var effectiveStart = currentTime > schedule.StartTime ? currentTime : schedule.StartTime;
                var availableMinutes = (int)(schedule.EndTime - effectiveStart).TotalMinutes;

                if (availableMinutes <= 0) continue;

                if (remaining <= availableMinutes) {
                    local = local.Date + effectiveStart.AddMinutes(remaining).ToTimeSpan();
                    remaining = 0;
                    break;
                }

                remaining -= availableMinutes;
            }

            if (remaining > 0) {
                local = local.Date.AddDays(1);
            }
        }

        return TimeZoneInfo.ConvertTimeToUtc(local, tz);
    }

    public static bool IsWithinBusinessHours(DateTime utcTime, BusinessHoursCalendar? calendar) {
        if (calendar is null || calendar.Schedules.Count == 0)
            return true;

        var tz = TimeZoneInfo.FindSystemTimeZoneById(calendar.TimeZoneId);
        var local = TimeZoneInfo.ConvertTimeFromUtc(utcTime, tz);
        var today = DateOnly.FromDateTime(local);

        if (calendar.Holidays.Any(h => h.Date == today))
            return false;

        var currentTime = TimeOnly.FromDateTime(local);
        return calendar.Schedules
            .Where(s => s.DayOfWeek == local.DayOfWeek)
            .Any(s => currentTime >= s.StartTime && currentTime < s.EndTime);
    }
}
