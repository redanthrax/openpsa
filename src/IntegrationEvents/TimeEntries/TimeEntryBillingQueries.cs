namespace IntegrationEvents.TimeEntries;

public record GetBillableTimeEntriesForClientQuery(Guid ClientId, DateTime? FromDate, DateTime? ToDate);

public record BillableTimeEntryData(
    Guid TimeEntryId,
    Guid ClientId,
    Guid? ProjectId,
    string? ProjectName,
    Guid? TicketId,
    string? TicketTitle,
    string UserName,
    DateTime Date,
    decimal Hours,
    string? Description);

public record GetBillableTimeEntriesForClientResponse(List<BillableTimeEntryData> Entries);

public record MarkTimeEntriesInvoicedCommand(List<Guid> TimeEntryIds);
