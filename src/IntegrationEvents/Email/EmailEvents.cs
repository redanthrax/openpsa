namespace IntegrationEvents.Email;

public record EmailReceived(Guid EmailMessageId, Guid MailboxConnectionId, string FromAddress, string Subject, Guid? TicketId);
public record EmailSent(Guid EmailMessageId, Guid? TicketId, string ToAddress, string Subject);
public record EmailDeliveryFailed(Guid EmailMessageId, string Error);
