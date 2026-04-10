namespace Contracts.Projects;

public class ProjectSummaryDto {
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ProjectStatus Status { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public decimal LoggedHours { get; set; }
    public decimal? BudgetHours { get; set; }
    public DateTime? EndDate { get; set; }
}
