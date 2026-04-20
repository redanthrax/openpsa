using Common.Domain;

namespace OpenPsa.Modules.Sla.Models;

public class BusinessHoursCalendar : BaseEntity {
    public string Name { get; set; } = string.Empty;
    public string TimeZoneId { get; set; } = "UTC";
    public bool IsDefault { get; set; }
    public List<BusinessHoursSchedule> Schedules { get; set; } = [];
    public List<BusinessHoursHoliday> Holidays { get; set; } = [];
}
