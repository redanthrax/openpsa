namespace Contracts.Agreements;

public class CreateAgreementRequest {
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public AgreementType Type { get; set; } = AgreementType.TimeAndMaterials;
    public Guid ClientId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal? MonthlyAmount { get; set; }
    public decimal? TotalValue { get; set; }
    public decimal? BlockHoursTotal { get; set; }
    public decimal? HourlyRate { get; set; }
    public int? RenewalNoticeDays { get; set; }
    public Guid? SlaPolicyId { get; set; }
}
