using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OpenPsa.Modules.Expenses.Models;
using Wolverine;

namespace OpenPsa.Modules.Expenses.Features.GetExpenseById;

public class GetExpenseByIdEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapGet("/api/expenses/{id:guid}", async (
            Guid id, OpenPsaDbContext db, IMessageBus bus, CancellationToken ct) => {

                var expense = await db.Set<Expense>().FindAsync([id], ct);
                if (expense is null)
                    return Results.Json(Result.Fail<object>("Expense not found"), statusCode: 404);

                var dto = await CreateExpense.CreateExpenseEndpoint.EnrichDto(expense, bus, ct);
                return Results.Ok(Result.Ok(dto));
            }).RequirePermission("expenses.view").WithTags("Expenses");
    }
}
