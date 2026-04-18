namespace Contracts.Tickets;

public class TicketQueueDto {
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TicketQueueAssignmentStrategy AssignmentStrategy { get; set; }
    public Guid? DefaultSlaPolicyId { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public int OpenTicketCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
