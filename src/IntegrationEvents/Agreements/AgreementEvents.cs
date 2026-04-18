namespace IntegrationEvents.Agreements;

public record AgreementCreated(Guid AgreementId, string Name, Guid ClientId);
public record AgreementUpdated(Guid AgreementId, string Name);
public record AgreementDeleted(Guid AgreementId);
