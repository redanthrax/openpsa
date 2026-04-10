using Common.Database;
using IntegrationEvents.Clients;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Clients.Models;

namespace OpenPsa.Modules.Clients.Features.Integration;

public class ClientNameQueryHandler {
    private readonly OpenPsaDbContext _db;
    public ClientNameQueryHandler(OpenPsaDbContext db) => _db = db;

    public async Task<GetClientNameResponse> Handle(GetClientNameQuery query) {
        var name = await _db.Set<Client>().Where(c => c.Id == query.ClientId)
            .Select(c => c.Name).FirstOrDefaultAsync();
        return name != null ? new(true, name) : new(false, null);
    }

    public async Task<GetClientNamesResponse> Handle(GetClientNamesQuery query) {
        var names = await _db.Set<Client>()
            .Where(c => query.ClientIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name);
        return new(names);
    }
}
