namespace Contracts.Agreements;

public class AgreementDto {
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public AgreementType Type { get; set; }
    public AgreementStatus Status { get; set; }
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal? MonthlyAmount { get; set; }
    public decimal? TotalValue { get; set; }
    public decimal? BlockHoursTotal { get; set; }
    public decimal? BlockHoursUsed { get; set; }
    public decimal? BlockHoursRemaining => BlockHoursTotal.HasValue ? BlockHoursTotal.Value - (BlockHoursUsed ?? 0) : null;
    public decimal? HourlyRate { get; set; }
    public int? RenewalNoticeDays { get; set; }
    public Guid? SlaPolicyId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
