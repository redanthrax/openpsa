namespace Contracts.Expenses;

public class ExpenseDto {
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public ExpenseCategory Category { get; set; }
    public ExpenseStatus Status { get; set; }
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public bool Billable { get; set; }
    public Guid? ClientId { get; set; }
    public string? ClientName { get; set; }
    public Guid? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public Guid? TicketId { get; set; }
    public string? TicketTitle { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? ReceiptPath { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
