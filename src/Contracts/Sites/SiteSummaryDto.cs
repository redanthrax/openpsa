namespace Contracts.Sites;

public class SiteSummaryDto {
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public string? ClientName { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Timezone { get; set; }
    public bool IsPrimary { get; set; }
}
