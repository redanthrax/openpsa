using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Expenses;
using Contracts.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OpenPsa.Modules.Expenses.Models;
using Wolverine;

namespace OpenPsa.Modules.Expenses.Features.UpdateExpense;

public class UpdateExpenseEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapPut("/api/expenses/{id:guid}", async (
            Guid id, UpdateExpenseRequest request, OpenPsaDbContext db, IMessageBus bus, CancellationToken ct) => {

                var expense = await db.Set<Expense>().FindAsync([id], ct);
                if (expense is null)
                    return Results.Json(Result.Fail<object>("Expense not found"), statusCode: 404);

                expense.Description = request.Description;
                expense.Category = request.Category;
                expense.Status = request.Status;
                expense.Amount = request.Amount;
                expense.ExpenseDate = request.ExpenseDate;
                expense.Billable = request.Billable;
                expense.ProjectId = request.ProjectId;
                expense.TicketId = request.TicketId;
                expense.Notes = request.Notes;
                expense.UpdatedAt = DateTime.UtcNow;

                await db.SaveChangesAsync(ct);

                var dto = await CreateExpense.CreateExpenseEndpoint.EnrichDto(expense, bus, ct);
                return Results.Ok(Result.Ok(dto));
            }).RequirePermission("expenses.update").WithTags("Expenses");
    }
}
