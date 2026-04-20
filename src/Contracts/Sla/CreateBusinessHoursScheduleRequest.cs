namespace Contracts.Sla;

public class CreateBusinessHoursScheduleRequest {
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}
