using Common.Domain;

namespace OpenPsa.Modules.Invoicing.Models;

public class InvoiceLineItem : BaseEntity {
    public Guid InvoiceId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Amount => Math.Round(Quantity * UnitPrice, 2);
}
