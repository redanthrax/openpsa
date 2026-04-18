namespace IntegrationEvents.Agreements;

public record GetAgreementNameQuery(Guid AgreementId);
public record GetAgreementNameResponse(bool Found, string? Name);

public record GetAgreementNamesQuery(IReadOnlyList<Guid> AgreementIds);
public record GetAgreementNamesResponse(Dictionary<Guid, string> Names);

public record GetActiveAgreementForClientQuery(Guid ClientId);
public record GetActiveAgreementForClientResponse(Guid? AgreementId, string? AgreementName, Guid? SlaPolicyId);
