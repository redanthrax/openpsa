using Common.Domain;

namespace OpenPsa.Modules.Settings.Models;

public class GeneralSettings : BaseEntity {
    public string CompanyName { get; set; } = string.Empty;
    public string? CompanyEmail { get; set; }
    public string? CompanyPhone { get; set; }
    public string? CompanyWebsite { get; set; }
    public string DefaultCurrency { get; set; } = "USD";
    public int DefaultPaymentTermsDays { get; set; } = 30;
}
