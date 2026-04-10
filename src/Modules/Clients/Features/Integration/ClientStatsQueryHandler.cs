using Common.Database;
using Contracts.Clients;
using IntegrationEvents.Clients;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Clients.Models;

namespace OpenPsa.Modules.Clients.Features.Integration;

public class ClientStatsQueryHandler {
    private readonly OpenPsaDbContext _db;
    public ClientStatsQueryHandler(OpenPsaDbContext db) => _db = db;

    public async Task<GetClientCountResponse> Handle(GetClientCountQuery query) {
        var count = await _db.Set<Client>().CountAsync(c => c.Status == ClientStatus.Active);
        return new(count);
    }
}
