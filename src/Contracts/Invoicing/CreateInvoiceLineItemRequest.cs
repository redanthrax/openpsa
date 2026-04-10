namespace Contracts.Invoicing;

public class CreateInvoiceLineItemRequest {
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
