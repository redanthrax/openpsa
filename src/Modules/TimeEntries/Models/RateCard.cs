using Common.Domain;

namespace OpenPsa.Modules.TimeEntries.Models;

public class RateCard : BaseEntity {
    public string Name { get; set; } = string.Empty;
    public Guid? ClientId { get; set; }
    public bool IsDefault { get; set; }
    public List<RateCardEntry> Entries { get; set; } = [];
}
