namespace IntegrationEvents.Tickets;

public record TicketCreated(Guid TicketId, string Title, Guid ClientId);
public record TicketUpdated(Guid TicketId, string Title);
public record TicketDeleted(Guid TicketId);
