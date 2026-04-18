using Common.Database;
using IntegrationEvents.Clients;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Clients.Models;

namespace OpenPsa.Modules.Clients.Features.Integration;

public class ClientDefaultQueryHandler {
    private readonly OpenPsaDbContext _db;
    public ClientDefaultQueryHandler(OpenPsaDbContext db) => _db = db;

    public async Task<GetDefaultClientResponse> Handle(GetDefaultClientQuery query) {
        var clientId = await _db.Set<Client>()
            .OrderBy(c => c.CreatedAt)
            .Select(c => c.Id)
            .FirstOrDefaultAsync();

        return new GetDefaultClientResponse(clientId);
    }
}
