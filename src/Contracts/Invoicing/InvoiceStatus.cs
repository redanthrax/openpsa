namespace Contracts.Invoicing;

public enum InvoiceStatus {
    Draft,
    Sent,
    Viewed,
    PartiallyPaid,
    Paid,
    Overdue,
    Void
}
