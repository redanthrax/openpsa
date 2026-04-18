using Common.Domain;
using Contracts.Expenses;

namespace OpenPsa.Modules.Expenses.Models;

public class Expense : BaseEntity {
    public string Description { get; set; } = string.Empty;
    public ExpenseCategory Category { get; set; }
    public ExpenseStatus Status { get; set; } = ExpenseStatus.Draft;
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public bool Billable { get; set; } = true;
    public Guid? ClientId { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid? TicketId { get; set; }
    public string? UserId { get; set; }
    public string? ReceiptPath { get; set; }
    public string? Notes { get; set; }
}
