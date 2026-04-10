namespace IntegrationEvents.Projects;

public record ProjectCreated(Guid ProjectId, string Name, Guid ClientId);
public record ProjectUpdated(Guid ProjectId, string Name);
public record ProjectDeleted(Guid ProjectId);
