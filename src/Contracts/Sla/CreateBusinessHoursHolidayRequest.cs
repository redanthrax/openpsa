namespace Contracts.Sla;

public class CreateBusinessHoursHolidayRequest {
    public string Name { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
}
