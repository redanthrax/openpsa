namespace Contracts.Expenses;

public class CreateExpenseRequest {
    public string Description { get; set; } = string.Empty;
    public ExpenseCategory Category { get; set; }
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; } = DateTime.Today;
    public bool Billable { get; set; } = true;
    public Guid? ClientId { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid? TicketId { get; set; }
    public string? Notes { get; set; }
}
