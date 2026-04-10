namespace IntegrationEvents.Authentication;

public record GetUserNameQuery(Guid UserId);
public record GetUserNameResponse(bool Found, string? Name);

public record GetUserNamesQuery(IReadOnlyList<Guid> UserIds);
public record GetUserNamesResponse(Dictionary<Guid, string> Names);
