using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Expenses;
using Contracts.Results;
using IntegrationEvents.Authentication;
using IntegrationEvents.Clients;
using IntegrationEvents.Projects;
using IntegrationEvents.Tickets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OpenPsa.Modules.Expenses.Models;
using Wolverine;

namespace OpenPsa.Modules.Expenses.Features.CreateExpense;

public class CreateExpenseEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapPost("/api/expenses", async (
            CreateExpenseRequest request, OpenPsaDbContext db, IMessageBus bus,
            HttpContext http, CancellationToken ct) => {

            var userId = http.User.FindFirst("sub")?.Value
                ?? http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var expense = new Expense {
                Description = request.Description,
                Category = request.Category,
                Amount = request.Amount,
                ExpenseDate = request.ExpenseDate,
                Billable = request.Billable,
                ClientId = request.ClientId,
                ProjectId = request.ProjectId,
                TicketId = request.TicketId,
                UserId = userId,
                Notes = request.Notes
            };

            db.Set<Expense>().Add(expense);
            await db.SaveChangesAsync(ct);

            var dto = await EnrichDto(expense, bus, ct);
            return Results.Created($"/api/expenses/{expense.Id}", Result.Ok(dto));
        }).RequirePermission("expenses.create").WithTags("Expenses");
    }

    internal static async Task<ExpenseDto> EnrichDto(Expense e, IMessageBus bus, CancellationToken ct) {
        var dto = new ExpenseDto {
            Id = e.Id,
            Description = e.Description,
            Category = e.Category,
            Status = e.Status,
            Amount = e.Amount,
            ExpenseDate = e.ExpenseDate,
            Billable = e.Billable,
            ClientId = e.ClientId,
            ProjectId = e.ProjectId,
            TicketId = e.TicketId,
            UserId = e.UserId,
            ReceiptPath = e.ReceiptPath,
            Notes = e.Notes,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt
        };

        if (e.ClientId.HasValue) {
            var r = await bus.InvokeAsync<GetClientNameResponse>(new GetClientNameQuery(e.ClientId.Value), ct);
            dto.ClientName = r.Name;
        }
        if (e.ProjectId.HasValue) {
            var r = await bus.InvokeAsync<GetProjectNameResponse>(new GetProjectNameQuery(e.ProjectId.Value), ct);
            dto.ProjectName = r.Name;
        }
        if (e.TicketId.HasValue) {
            var r = await bus.InvokeAsync<GetTicketTitleResponse>(new GetTicketTitleQuery(e.TicketId.Value), ct);
            dto.TicketTitle = r.Title;
        }
        if (e.UserId != null && Guid.TryParse(e.UserId, out var uid)) {
            var r = await bus.InvokeAsync<GetUserNameResponse>(new GetUserNameQuery(uid), ct);
            dto.UserName = r.Name;
        }

        return dto;
    }
}
