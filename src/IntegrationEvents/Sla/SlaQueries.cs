namespace IntegrationEvents.Sla;

public record GetSlaPolicyQuery(Guid SlaPolicyId);
public record GetSlaPolicyResponse(bool Found, string? Name, Dictionary<int, SlaPolicyTargetData>? Targets);

public record SlaPolicyTargetData(int ResponseTimeMinutes, int ResolutionTimeMinutes);

public record GetDefaultSlaPolicyQuery;
public record GetDefaultSlaPolicyResponse(Guid? PolicyId, string? Name);
