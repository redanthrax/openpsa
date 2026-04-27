using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Expenses;
using Contracts.Results;
using IntegrationEvents.Authentication;
using IntegrationEvents.Clients;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Expenses.Models;
using Wolverine;

namespace OpenPsa.Modules.Expenses.Features.GetAllExpenses;

public class GetAllExpensesEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapGet("/api/expenses", async (
            OpenPsaDbContext db, IMessageBus bus,
            Guid? clientId, Guid? projectId, ExpenseStatus? status,
            int page = 1, int pageSize = 25,
            CancellationToken ct = default) => {

                var query = db.Set<Expense>().AsQueryable();
                if (clientId.HasValue) query = query.Where(e => e.ClientId == clientId.Value);
                if (projectId.HasValue) query = query.Where(e => e.ProjectId == projectId.Value);
                if (status.HasValue) query = query.Where(e => e.Status == status.Value);

                var ordered = query.OrderByDescending(e => e.ExpenseDate);
                var totalCount = await ordered.CountAsync(ct);
                var expenses = await ordered
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(ct);

                var clientIds = expenses.Where(e => e.ClientId.HasValue).Select(e => e.ClientId!.Value).Distinct().ToList();
                var clientNames = clientIds.Count > 0
                    ? (await bus.InvokeAsync<GetClientNamesResponse>(new GetClientNamesQuery(clientIds), ct)).Names
                    : new Dictionary<Guid, string>();

                var userIds = expenses.Where(e => e.UserId != null && Guid.TryParse(e.UserId, out _))
                    .Select(e => Guid.Parse(e.UserId!)).Distinct().ToList();
                var userNames = userIds.Count > 0
                    ? (await bus.InvokeAsync<GetUserNamesResponse>(new GetUserNamesQuery(userIds), ct)).Names
                    : new Dictionary<Guid, string>();

                var dtos = expenses.Select(e => new ExpenseSummaryDto {
                    Id = e.Id,
                    Description = e.Description,
                    Category = e.Category,
                    Status = e.Status,
                    Amount = e.Amount,
                    ExpenseDate = e.ExpenseDate,
                    Billable = e.Billable,
                    ClientName = e.ClientId.HasValue ? clientNames.GetValueOrDefault(e.ClientId.Value) : null,
                    UserName = e.UserId != null && Guid.TryParse(e.UserId, out var uid) ? userNames.GetValueOrDefault(uid) : null
                }).ToList();

                return Results.Ok(PagedResult.Ok<ExpenseSummaryDto>(dtos, totalCount, page, pageSize));
            }).RequirePermission("expenses.list").WithTags("Expenses");
    }
}
