namespace Contracts.TimeEntries;

public class CreateRateCardEntryRequest {
    public string ServiceType { get; set; } = string.Empty;
    public decimal HourlyRate { get; set; }
    public decimal? AfterHoursRate { get; set; }
}
