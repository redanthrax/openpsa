using Common.Audit;
using Common.Database;
using Common.Modules;
using Contracts.Dashboard;
using Contracts.Results;
using IntegrationEvents.Clients;
using IntegrationEvents.Invoicing;
using IntegrationEvents.Projects;
using IntegrationEvents.Tickets;
using IntegrationEvents.TimeEntries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace OpenPsa.Modules.Dashboard.Features.GetDashboardStats;

public class GetDashboardStatsEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapGet("/api/dashboard/stats", async (OpenPsaDbContext db, IMessageBus bus, CancellationToken ct) => {
            var clientCount = (await bus.InvokeAsync<GetClientCountResponse>(new GetClientCountQuery(), ct)).Count;
            var activeProjects = (await bus.InvokeAsync<GetActiveProjectCountResponse>(new GetActiveProjectCountQuery(), ct)).Count;
            var ticketStats = await bus.InvokeAsync<GetTicketStatsResponse>(new GetTicketStatsQuery(), ct);
            var unbilledHours = (await bus.InvokeAsync<GetUnbilledHoursResponse>(new GetUnbilledHoursQuery(), ct)).Hours;
            var outstandingTotal = (await bus.InvokeAsync<GetOutstandingInvoicesTotalResponse>(new GetOutstandingInvoicesTotalQuery(), ct)).Total;

            var recentEntries = await db.Set<AuditEntry>()
                .Where(a => a.EntityName == "Client" || a.EntityName == "Contact" ||
                            a.EntityName == "Project" || a.EntityName == "Ticket")
                .OrderByDescending(a => a.CreatedAt)
                .Take(10)
                .ToListAsync(ct);

            var recentActivity = recentEntries.Select(a => new RecentActivityDto {
                Action = a.Action.ToString(),
                Description = $"{a.Action} {a.EntityName} {a.EntityId}",
                UserName = a.UserName ?? string.Empty,
                CreatedAt = a.CreatedAt
            }).ToList();

            var stats = new DashboardStatsDto {
                TotalClients = clientCount,
                ActiveProjects = activeProjects,
                OpenTickets = ticketStats.OpenCount,
                OverdueTickets = ticketStats.OverdueCount,
                UnbilledHours = unbilledHours,
                OutstandingInvoices = outstandingTotal,
                RecentActivity = recentActivity
            };

            return Results.Ok(Result.Ok(stats));
        }).WithTags("Dashboard");
    }
}
