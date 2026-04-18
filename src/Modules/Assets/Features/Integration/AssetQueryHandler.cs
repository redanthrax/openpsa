using Common.Database;
using IntegrationEvents.Assets;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Assets.Models;

namespace OpenPsa.Modules.Assets.Features.Integration;

public class AssetQueryHandler {
    private readonly OpenPsaDbContext _db;
    public AssetQueryHandler(OpenPsaDbContext db) => _db = db;

    public async Task<GetAssetNameResponse> Handle(GetAssetNameQuery query) {
        var name = await _db.Set<Asset>().Where(a => a.Id == query.AssetId)
            .Select(a => a.Name).FirstOrDefaultAsync();
        return name != null ? new(true, name) : new(false, null);
    }

    public async Task<GetAssetNamesResponse> Handle(GetAssetNamesQuery query) {
        var names = await _db.Set<Asset>()
            .Where(a => query.AssetIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, a => a.Name);
        return new(names);
    }

    public async Task<GetAssetCountForClientResponse> Handle(GetAssetCountForClientQuery query) {
        var count = await _db.Set<Asset>().CountAsync(a => a.ClientId == query.ClientId);
        return new(count);
    }
}
