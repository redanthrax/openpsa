namespace IntegrationEvents.Assets;

public record GetAssetNameQuery(Guid AssetId);
public record GetAssetNameResponse(bool Found, string? Name);

public record GetAssetNamesQuery(IReadOnlyList<Guid> AssetIds);
public record GetAssetNamesResponse(Dictionary<Guid, string> Names);

public record GetAssetCountForClientQuery(Guid ClientId);
public record GetAssetCountForClientResponse(int Count);
