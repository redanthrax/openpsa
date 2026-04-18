namespace Contracts.Tickets;

public class UpdateTicketQueueRequest {
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TicketQueueAssignmentStrategy AssignmentStrategy { get; set; }
    public Guid? DefaultSlaPolicyId { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}
