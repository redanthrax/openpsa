namespace Contracts.Agreements;

public class AgreementSummaryDto {
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public AgreementType Type { get; set; }
    public AgreementStatus Status { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal? MonthlyAmount { get; set; }
    public decimal? BlockHoursRemaining { get; set; }
}
