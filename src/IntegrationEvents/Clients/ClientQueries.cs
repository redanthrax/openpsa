namespace IntegrationEvents.Clients;

public record GetClientNameQuery(Guid ClientId);
public record GetClientNameResponse(bool Found, string? Name);

public record GetClientNamesQuery(IReadOnlyList<Guid> ClientIds);
public record GetClientNamesResponse(Dictionary<Guid, string> Names);
