namespace Contracts.Sla;

public class SlaTargetDto {
    public Guid Id { get; set; }
    public SlaPriorityLevel Priority { get; set; }
    public int ResponseTimeMinutes { get; set; }
    public int ResolutionTimeMinutes { get; set; }
}
