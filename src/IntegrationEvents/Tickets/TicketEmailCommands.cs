using Contracts.Tickets;

namespace IntegrationEvents.Tickets;

public record CreateTicketFromEmailCommand(string Title, string? Description, TicketPriority Priority, TicketType Type, Guid ClientId, Guid? QueueId);
public record TicketCreatedResponse(Guid TicketId);
