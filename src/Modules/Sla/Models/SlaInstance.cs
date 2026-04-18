using Common.Domain;
using Contracts.Sla;

namespace OpenPsa.Modules.Sla.Models;

public class SlaInstance : BaseEntity {
    public Guid TicketId { get; set; }
    public Guid SlaPolicyId { get; set; }
    public SlaPriorityLevel Priority { get; set; }
    public DateTime? ResponseDueAt { get; set; }
    public DateTime? ResolutionDueAt { get; set; }
    public DateTime? RespondedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public bool ResponseBreached { get; set; }
    public bool ResolutionBreached { get; set; }
    public bool IsPaused { get; set; }
    public DateTime? PausedAt { get; set; }
    public int PausedMinutes { get; set; }
}
