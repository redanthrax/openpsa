namespace Contracts.Tickets;

public class CreateTicketQueueRequest {
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TicketQueueAssignmentStrategy AssignmentStrategy { get; set; } = TicketQueueAssignmentStrategy.Manual;
    public Guid? DefaultSlaPolicyId { get; set; }
    public int SortOrder { get; set; }
}
