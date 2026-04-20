namespace IntegrationEvents.Tickets;

public record GetTicketStatsQuery;
public record GetTicketStatsResponse(int OpenCount, int OverdueCount);

public record GetOpenTicketCountsByClientQuery(List<Guid> ClientIds);
public record GetOpenTicketCountsByClientResponse(Dictionary<Guid, int> Counts);
