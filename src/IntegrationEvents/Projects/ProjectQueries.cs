namespace IntegrationEvents.Projects;

public record GetProjectNameQuery(Guid ProjectId);
public record GetProjectNameResponse(bool Found, string? Name);

public record GetProjectNamesQuery(IReadOnlyList<Guid> ProjectIds);
public record GetProjectNamesResponse(Dictionary<Guid, string> Names);
