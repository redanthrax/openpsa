using Common.Domain;

namespace OpenPsa.Modules.Tickets.Models;

public class TicketQueueMember : BaseEntity {
    public Guid QueueId { get; set; }
    public string UserId { get; set; } = string.Empty;
}
