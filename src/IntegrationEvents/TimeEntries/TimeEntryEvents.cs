namespace IntegrationEvents.TimeEntries;

public record TimeEntryLogged(Guid TimeEntryId, Guid ClientId, Guid? ProjectId, decimal Hours, bool Billable, bool Invoiced);
public record TimeEntryUpdated(Guid TimeEntryId, Guid? ProjectId, decimal Hours, bool Billable, bool Invoiced);
public record TimeEntryDeleted(Guid TimeEntryId, Guid? ProjectId, decimal Hours);
