namespace Contracts.TimeEntries;

public class RateCardDto {
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? ClientId { get; set; }
    public string? ClientName { get; set; }
    public bool IsDefault { get; set; }
    public List<RateCardEntryDto> Entries { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
