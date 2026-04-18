using Common.Database;
using IntegrationEvents.Clients;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Contacts.Models;
using Wolverine;

namespace OpenPsa.Modules.Contacts.Features.Integration;

public class ContactEmailQueryHandler {
    private readonly OpenPsaDbContext _db;
    private readonly IMessageBus _bus;

    public ContactEmailQueryHandler(OpenPsaDbContext db, IMessageBus bus) {
        _db = db;
        _bus = bus;
    }

    public async Task<FindClientByContactEmailResponse> Handle(FindClientByContactEmailQuery query) {
        var contact = await _db.Set<Contact>()
            .Where(c => c.Email == query.EmailAddress)
            .Select(c => new { c.ClientId })
            .FirstOrDefaultAsync();

        if (contact is null)
            return new FindClientByContactEmailResponse(false, null, null);

        var clientNameResponse = await _bus.InvokeAsync<GetClientNameResponse>(
            new GetClientNameQuery(contact.ClientId));

        return new FindClientByContactEmailResponse(true, contact.ClientId, clientNameResponse.Name);
    }
}
