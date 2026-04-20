namespace IntegrationEvents.Projects;

public record GetActiveProjectCountQuery;
public record GetActiveProjectCountResponse(int Count);

public record GetActiveProjectCountsByClientQuery(List<Guid> ClientIds);
public record GetActiveProjectCountsByClientResponse(Dictionary<Guid, int> Counts);
