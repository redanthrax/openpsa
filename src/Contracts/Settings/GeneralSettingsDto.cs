namespace Contracts.Settings;

public class GeneralSettingsDto {
    public string CompanyName { get; set; } = string.Empty;
    public string? CompanyEmail { get; set; }
    public string? CompanyPhone { get; set; }
    public string? CompanyWebsite { get; set; }
    public string? DefaultCurrency { get; set; }
    public int DefaultPaymentTermsDays { get; set; }
}
