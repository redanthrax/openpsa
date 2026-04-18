namespace Contracts.Sla;

public class CreateSlaPolicyRequest {
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
    public List<CreateSlaTargetRequest> Targets { get; set; } = [];
}
