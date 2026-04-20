namespace Contracts.Sla;

public class BusinessHoursCalendarDto {
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TimeZoneId { get; set; } = "UTC";
    public bool IsDefault { get; set; }
    public List<BusinessHoursScheduleDto> Schedules { get; set; } = [];
    public List<BusinessHoursHolidayDto> Holidays { get; set; } = [];
    public DateTime CreatedAt { get; set; }
}
