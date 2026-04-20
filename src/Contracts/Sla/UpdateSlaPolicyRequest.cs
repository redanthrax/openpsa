namespace Contracts.Sla;

public class UpdateSlaPolicyRequest {
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
    public Guid? BusinessHoursCalendarId { get; set; }
    public List<CreateSlaTargetRequest> Targets { get; set; } = [];
}
