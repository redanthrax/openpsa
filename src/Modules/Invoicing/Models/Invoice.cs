using Common.Domain;
using Contracts.Invoicing;

namespace OpenPsa.Modules.Invoicing.Models;

public class Invoice : BaseEntity {
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid ClientId { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal TaxRate { get; set; }
    public decimal AmountPaid { get; set; }
    public string? Notes { get; set; }
    public List<InvoiceLineItem> LineItems { get; set; } = [];

    public decimal Subtotal => LineItems.Sum(l => l.Amount);
    public decimal TaxAmount => Math.Round(Subtotal * TaxRate / 100, 2);
    public decimal Total => Subtotal + TaxAmount;
    public decimal AmountDue => Total - AmountPaid;
}
