namespace Contracts.Sla;

public class CreateSlaTargetRequest {
    public SlaPriorityLevel Priority { get; set; }
    public int ResponseTimeMinutes { get; set; }
    public int ResolutionTimeMinutes { get; set; }
}
