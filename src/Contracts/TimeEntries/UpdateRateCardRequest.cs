namespace Contracts.TimeEntries;

public class UpdateRateCardRequest {
    public string Name { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public List<CreateRateCardEntryRequest> Entries { get; set; } = [];
}
