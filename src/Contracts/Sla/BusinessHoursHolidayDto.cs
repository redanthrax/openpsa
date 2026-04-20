namespace Contracts.Sla;

public class BusinessHoursHolidayDto {
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
}
