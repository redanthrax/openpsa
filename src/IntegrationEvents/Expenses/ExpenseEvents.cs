namespace IntegrationEvents.Expenses;

public record ExpenseCreated(Guid ExpenseId, Guid? ClientId, decimal Amount, bool Billable);
public record ExpenseUpdated(Guid ExpenseId, decimal Amount, bool Billable);
public record ExpenseDeleted(Guid ExpenseId);
