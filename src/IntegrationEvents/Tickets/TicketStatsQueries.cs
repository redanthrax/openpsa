namespace IntegrationEvents.Tickets;

public record GetTicketStatsQuery;
public record GetTicketStatsResponse(int OpenCount, int OverdueCount);
