namespace Contracts.Sla;

public class SlaInstanceDto {
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public Guid SlaPolicyId { get; set; }
    public string SlaPolicyName { get; set; } = string.Empty;
    public SlaPriorityLevel Priority { get; set; }
    public DateTime? ResponseDueAt { get; set; }
    public DateTime? ResolutionDueAt { get; set; }
    public DateTime? RespondedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public bool ResponseBreached { get; set; }
    public bool ResolutionBreached { get; set; }
    public SlaStatus ResponseStatus { get; set; }
    public SlaStatus ResolutionStatus { get; set; }
    public bool IsPaused { get; set; }
    public int PausedMinutes { get; set; }
}
