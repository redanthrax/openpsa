namespace IntegrationEvents.Contacts;

public record CreateContactFromEmailCommand(string EmailAddress, string? DisplayName, Guid ClientId);
public record CreateContactFromEmailResponse(Guid ContactId);
