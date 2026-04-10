using Common.Domain;
using Contracts.Projects;

namespace OpenPsa.Modules.Projects.Models;

public class Project : BaseEntity {
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Planning;
    public Guid ClientId { get; set; }
    public string? ManagerUserId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? BudgetHours { get; set; }
    public decimal? BudgetAmount { get; set; }
    public decimal LoggedHours { get; set; }
}
