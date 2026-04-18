namespace IntegrationEvents.Assets;

public record AssetCreated(Guid AssetId, string Name, Guid ClientId);
public record AssetUpdated(Guid AssetId, string Name);
public record AssetDeleted(Guid AssetId);
