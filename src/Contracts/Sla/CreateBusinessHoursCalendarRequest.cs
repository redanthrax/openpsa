namespace Contracts.Sla;

public class CreateBusinessHoursCalendarRequest {
    public string Name { get; set; } = string.Empty;
    public string TimeZoneId { get; set; } = "UTC";
    public bool IsDefault { get; set; }
    public List<CreateBusinessHoursScheduleRequest> Schedules { get; set; } = [];
    public List<CreateBusinessHoursHolidayRequest> Holidays { get; set; } = [];
}
