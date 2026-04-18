using Common.Domain;
using Contracts.Tickets;

namespace OpenPsa.Modules.Tickets.Models;

public class TicketQueue : BaseEntity {
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TicketQueueAssignmentStrategy AssignmentStrategy { get; set; } = TicketQueueAssignmentStrategy.Manual;
    public Guid? DefaultSlaPolicyId { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public int LastAssignedIndex { get; set; }
}
