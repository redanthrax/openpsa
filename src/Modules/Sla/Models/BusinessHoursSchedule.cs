using Common.Domain;

namespace OpenPsa.Modules.Sla.Models;

public class BusinessHoursSchedule : BaseEntity {
    public Guid CalendarId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}
