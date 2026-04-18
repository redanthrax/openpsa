using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OpenPsa.Modules.Expenses.Models;

namespace OpenPsa.Modules.Expenses.Features.DeleteExpense;

public class DeleteExpenseEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapDelete("/api/expenses/{id:guid}", async (Guid id, OpenPsaDbContext db, CancellationToken ct) => {
            var expense = await db.Set<Expense>().FindAsync([id], ct);
            if (expense is null)
                return Results.Json(Result.Fail<object>("Expense not found"), statusCode: 404);

            db.Set<Expense>().Remove(expense);
            await db.SaveChangesAsync(ct);
            return Results.Ok(Result.Ok<object?>(null));
        }).RequirePermission("expenses.delete").WithTags("Expenses");
    }
}
