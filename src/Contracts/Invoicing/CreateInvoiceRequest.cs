namespace Contracts.Invoicing;

public class CreateInvoiceRequest {
    public Guid ClientId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public string? Notes { get; set; }
    public List<CreateInvoiceLineItemRequest> LineItems { get; set; } = [];
}
