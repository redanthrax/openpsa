using Common.Database;
using Contracts.Tickets;
using IntegrationEvents.Tickets;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Tickets.Models;

namespace OpenPsa.Modules.Tickets.Features.Integration;

public class TicketStatsQueryHandler {
    private readonly OpenPsaDbContext _db;
    public TicketStatsQueryHandler(OpenPsaDbContext db) => _db = db;

    public async Task<GetTicketStatsResponse> Handle(GetTicketStatsQuery query) {
        var openStatuses = new[] { TicketStatus.New, TicketStatus.Open, TicketStatus.InProgress, TicketStatus.PendingCustomer };
        var now = DateTime.UtcNow;

        var openCount = await _db.Set<Ticket>().CountAsync(t => openStatuses.Contains(t.Status));
        var overdueCount = await _db.Set<Ticket>().CountAsync(t =>
            openStatuses.Contains(t.Status) && t.DueDate != null && t.DueDate < now);

        return new(openCount, overdueCount);
    }
}
