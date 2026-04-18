namespace Contracts.Assets;

public class AssetSummaryDto {
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public AssetType Type { get; set; }
    public AssetStatus Status { get; set; }
    public string? ClientName { get; set; }
    public string? SerialNumber { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public DateTime? WarrantyExpiry { get; set; }
}
