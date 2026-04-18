namespace Contracts.TimeEntries;

public class CreateRateCardRequest {
    public string Name { get; set; } = string.Empty;
    public Guid? ClientId { get; set; }
    public bool IsDefault { get; set; }
    public List<CreateRateCardEntryRequest> Entries { get; set; } = [];
}
