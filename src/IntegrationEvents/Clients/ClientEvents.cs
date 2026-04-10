namespace IntegrationEvents.Clients;

public record ClientCreated(Guid ClientId, string Name);
public record ClientUpdated(Guid ClientId, string Name);
public record ClientDeleted(Guid ClientId);
