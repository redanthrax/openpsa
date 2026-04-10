namespace Contracts.Projects;

public class UpdateProjectRequest {
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProjectStatus Status { get; set; }
    public string? ManagerUserId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? BudgetHours { get; set; }
    public decimal? BudgetAmount { get; set; }
}
