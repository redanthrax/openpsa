namespace IntegrationEvents.Email;

public record GetMailboxConnectionQuery(Guid MailboxConnectionId);
public record GetMailboxConnectionResponse(bool Found, Guid Id, string EmailAddress, string? SmtpHost, int? SmtpPort, bool SmtpUseSsl, string? SmtpUsername, string? EncryptedSmtpPassword, string? GraphTenantId, string? GraphClientId, string? EncryptedGraphClientSecret, string? GraphMailboxUserId, Contracts.Email.MailboxProvider Provider);

public record FindMailboxByEmailQuery(string EmailAddress);
public record FindMailboxByEmailResponse(bool Found, Guid? Id, Guid? DefaultQueueId, bool AutoCreateContacts, Contracts.Email.MailboxProvider Provider);

public record GetMailboxForTicketQuery(Guid TicketId);
public record GetMailboxForTicketResponse(bool Found, Guid? MailboxConnectionId);
