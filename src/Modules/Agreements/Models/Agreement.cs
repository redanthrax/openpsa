using Common.Domain;
using Contracts.Agreements;

namespace OpenPsa.Modules.Agreements.Models;

public class Agreement : BaseEntity {
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public AgreementType Type { get; set; } = AgreementType.TimeAndMaterials;
    public AgreementStatus Status { get; set; } = AgreementStatus.Draft;
    public Guid ClientId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal? MonthlyAmount { get; set; }
    public decimal? TotalValue { get; set; }
    public decimal? BlockHoursTotal { get; set; }
    public decimal? BlockHoursUsed { get; set; }
    public decimal? HourlyRate { get; set; }
    public int? RenewalNoticeDays { get; set; }
    public Guid? SlaPolicyId { get; set; }
}
