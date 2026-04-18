using Common.Domain;

namespace OpenPsa.Modules.TimeEntries.Models;

public class RateCardEntry : BaseEntity {
    public Guid RateCardId { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public decimal HourlyRate { get; set; }
    public decimal? AfterHoursRate { get; set; }
}
