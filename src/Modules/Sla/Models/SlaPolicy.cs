using Common.Domain;

namespace OpenPsa.Modules.Sla.Models;

public class SlaPolicy : BaseEntity {
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
    public Guid? BusinessHoursCalendarId { get; set; }
    public List<SlaTarget> Targets { get; set; } = [];
}
