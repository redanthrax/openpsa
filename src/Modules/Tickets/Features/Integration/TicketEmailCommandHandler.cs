using Common.Database;
using IntegrationEvents.Tickets;
using OpenPsa.Modules.Tickets.Models;

namespace OpenPsa.Modules.Tickets.Features.Integration;

public class TicketEmailCommandHandler {
    private readonly OpenPsaDbContext _db;
    public TicketEmailCommandHandler(OpenPsaDbContext db) => _db = db;

    public async Task<TicketCreatedResponse> Handle(CreateTicketFromEmailCommand command) {
        var ticket = new Ticket {
            Title = command.Title,
            Description = command.Description,
            Priority = command.Priority,
            Type = command.Type,
            ClientId = command.ClientId,
            QueueId = command.QueueId
        };

        _db.Set<Ticket>().Add(ticket);
        await _db.SaveChangesAsync();
        return new TicketCreatedResponse(ticket.Id);
    }
}
