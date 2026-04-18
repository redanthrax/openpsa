namespace Contracts.Expenses;

public class UpdateExpenseRequest {
    public string Description { get; set; } = string.Empty;
    public ExpenseCategory Category { get; set; }
    public ExpenseStatus Status { get; set; }
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public bool Billable { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid? TicketId { get; set; }
    public string? Notes { get; set; }
}
