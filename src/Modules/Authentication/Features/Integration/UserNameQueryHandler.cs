using Common.Database;
using IntegrationEvents.Authentication;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Authentication.Models;

namespace OpenPsa.Modules.Authentication.Features.Integration;

public class UserNameQueryHandler {
    private readonly OpenPsaDbContext _db;
    public UserNameQueryHandler(OpenPsaDbContext db) => _db = db;

    public async Task<GetUserNameResponse> Handle(GetUserNameQuery query) {
        var name = await _db.Set<User>().Where(u => u.Id == query.UserId)
            .Select(u => u.Name).FirstOrDefaultAsync();
        return name != null ? new(true, name) : new(false, null);
    }

    public async Task<GetUserNamesResponse> Handle(GetUserNamesQuery query) {
        var names = await _db.Set<User>()
            .Where(u => query.UserIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Name);
        return new(names);
    }
}
