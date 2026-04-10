namespace IntegrationEvents.Tickets;

public record GetTicketTitleQuery(Guid TicketId);
public record GetTicketTitleResponse(bool Found, string? Title);

public record GetTicketTitlesQuery(IReadOnlyList<Guid> TicketIds);
public record GetTicketTitlesResponse(Dictionary<Guid, string> Titles);
