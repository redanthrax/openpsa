namespace Contracts.Expenses;

public class ExpenseSummaryDto {
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public ExpenseCategory Category { get; set; }
    public ExpenseStatus Status { get; set; }
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public bool Billable { get; set; }
    public string? ClientName { get; set; }
    public string? ProjectName { get; set; }
    public string? UserName { get; set; }
}
