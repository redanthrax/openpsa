using System.Collections.Generic;
using OpenPsa.Modules.Sla.Models;
using OpenPsa.Modules.Sla.Services;
using Xunit;

namespace Api.Tests;

public class BusinessHoursServiceTests {
    [Fact]
    public void IsWithinBusinessHours_NoCalendar_ReturnsTrue() {
        var utcTime = DateTime.UtcNow;
        var result = BusinessHoursService.IsWithinBusinessHours(utcTime, null);
        Assert.True(result);
    }

    [Fact]
    public void IsWithinBusinessHours_OnHoliday_ReturnsFalse() {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var calendar = new BusinessHoursCalendar {
            TimeZoneId = "UTC",
            Holidays = new List<BusinessHoursHoliday> { new BusinessHoursHoliday { Date = today } },
            Schedules = new List<BusinessHoursSchedule> {
                new BusinessHoursSchedule {
                    DayOfWeek = DateTime.UtcNow.DayOfWeek,
                    StartTime = new TimeOnly(0, 0),
                    EndTime = new TimeOnly(23, 59),
                }
            }
        };
        var utcTime = DateTime.UtcNow;
        var result = BusinessHoursService.IsWithinBusinessHours(utcTime, calendar);
        Assert.False(result);
    }

    [Fact]
    public void CalculateDeadline_ZeroMinutes_ReturnsSameInstant() {
        var start = DateTime.UtcNow;
        var result = BusinessHoursService.CalculateDeadline(start, 0, null);
        Assert.Equal(start, result);
    }

    [Fact]
    public void CalculateDeadline_SkipsWeekendsAndHolidays() {
        var calendar = new BusinessHoursCalendar {
            TimeZoneId = "UTC",
            Schedules = new List<BusinessHoursSchedule> {
                new BusinessHoursSchedule { DayOfWeek = DayOfWeek.Monday, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(17, 0) },
                new BusinessHoursSchedule { DayOfWeek = DayOfWeek.Tuesday, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(17, 0) },
                new BusinessHoursSchedule { DayOfWeek = DayOfWeek.Wednesday, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(17, 0) },
                new BusinessHoursSchedule { DayOfWeek = DayOfWeek.Thursday, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(17, 0) },
                new BusinessHoursSchedule { DayOfWeek = DayOfWeek.Friday, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(17, 0) }
            },
            Holidays = new List<BusinessHoursHoliday> { new BusinessHoursHoliday { Date = new DateOnly(2023, 10, 9) } } // Monday holiday
        };

        // Start Friday Oct 6, 2023 17:00 UTC (after close)
        var start = new DateTime(2023, 10, 6, 17, 0, 0, DateTimeKind.Utc);
        // Add 480 minutes (8 hours), should skip weekend and holiday Monday, go to Tuesday Oct 10, 17:00
        // From Friday 17:00, skip to Monday, but Monday holiday, skip to Tuesday 9:00 + 480 min = Tuesday 17:00
        var expected = new DateTime(2023, 10, 10, 17, 0, 0, DateTimeKind.Utc);
        var result = BusinessHoursService.CalculateDeadline(start, 480, calendar);
        Assert.Equal(expected, result);
    }
}
