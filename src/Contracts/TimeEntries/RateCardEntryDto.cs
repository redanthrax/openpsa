namespace Contracts.TimeEntries;

public class RateCardEntryDto {
    public Guid Id { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public decimal HourlyRate { get; set; }
    public decimal? AfterHoursRate { get; set; }
}
