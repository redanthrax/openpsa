namespace Contracts.Tickets;

public class CreateTicketRequest {
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    public TicketType Type { get; set; } = TicketType.Incident;
    public Guid ClientId { get; set; }
    public Guid? ProjectId { get; set; }
    public string? AssignedToUserId { get; set; }
    public DateTime? DueDate { get; set; }
}
