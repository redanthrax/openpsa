using Common.Domain;

namespace OpenPsa.Modules.Sla.Models;

public class BusinessHoursHoliday : BaseEntity {
    public Guid CalendarId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
}
