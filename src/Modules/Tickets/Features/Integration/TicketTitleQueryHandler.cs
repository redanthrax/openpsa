using Common.Database;
using IntegrationEvents.Tickets;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Tickets.Models;

namespace OpenPsa.Modules.Tickets.Features.Integration;

public class TicketTitleQueryHandler {
    private readonly OpenPsaDbContext _db;
    public TicketTitleQueryHandler(OpenPsaDbContext db) => _db = db;

    public async Task<GetTicketTitleResponse> Handle(GetTicketTitleQuery query) {
        var title = await _db.Set<Ticket>().Where(t => t.Id == query.TicketId)
            .Select(t => t.Title).FirstOrDefaultAsync();
        return title != null ? new(true, title) : new(false, null);
    }

    public async Task<GetTicketTitlesResponse> Handle(GetTicketTitlesQuery query) {
        var titles = await _db.Set<Ticket>()
            .Where(t => query.TicketIds.Contains(t.Id))
            .ToDictionaryAsync(t => t.Id, t => t.Title);
        return new(titles);
    }
}
