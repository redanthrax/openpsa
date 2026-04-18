namespace IntegrationEvents.Clients;

public record GetClientNameQuery(Guid ClientId);
public record GetClientNameResponse(bool Found, string? Name);

public record GetClientNamesQuery(IReadOnlyList<Guid> ClientIds);
public record GetClientNamesResponse(Dictionary<Guid, string> Names);

public record FindClientByContactEmailQuery(string EmailAddress);
public record FindClientByContactEmailResponse(bool Found, Guid? ClientId, string? ClientName);

public record GetDefaultClientQuery();
public record GetDefaultClientResponse(Guid ClientId);
