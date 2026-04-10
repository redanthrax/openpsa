using Common.Domain;
using Contracts.Tickets;

namespace OpenPsa.Modules.Tickets.Models;

public class Ticket : BaseEntity {
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TicketStatus Status { get; set; } = TicketStatus.New;
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    public TicketType Type { get; set; } = TicketType.Incident;
    public Guid ClientId { get; set; }
    public Guid? ProjectId { get; set; }
    public string? AssignedToUserId { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ResolvedAt { get; set; }
}
