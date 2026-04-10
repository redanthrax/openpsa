namespace Contracts.Dashboard;

public class RecentActivityDto {
    public string Action { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
