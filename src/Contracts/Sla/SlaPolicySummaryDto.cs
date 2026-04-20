namespace Contracts.Sla;

public class SlaPolicySummaryDto {
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public string? BusinessHoursCalendarName { get; set; }
    public int TargetCount { get; set; }
}
