namespace Contracts.Tickets;

public class TicketSummaryDto {
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public TicketStatus Status { get; set; }
    public TicketPriority Priority { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string? AssignedToUserName { get; set; }
    public string? QueueName { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
