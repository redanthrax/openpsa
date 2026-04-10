namespace Contracts.Tickets;

public class UpdateTicketRequest {
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TicketStatus Status { get; set; }
    public TicketPriority Priority { get; set; }
    public TicketType Type { get; set; }
    public Guid? ProjectId { get; set; }
    public string? AssignedToUserId { get; set; }
    public DateTime? DueDate { get; set; }
}
