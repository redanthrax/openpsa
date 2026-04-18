namespace Contracts.Invoicing;

public class GenerateInvoiceFromTimeRequest {
    public Guid ClientId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public decimal? DefaultHourlyRate { get; set; }
    public int PaymentTermsDays { get; set; } = 30;
}
