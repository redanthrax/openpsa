namespace Contracts.Invoicing;

public class InvoiceSummaryDto {
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public InvoiceStatus Status { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Total { get; set; }
    public decimal AmountDue { get; set; }
}
