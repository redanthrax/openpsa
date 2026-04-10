namespace Contracts.TimeEntries;

public class CreateTimeEntryRequest {
    public Guid ClientId { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid? TicketId { get; set; }
    public DateTime Date { get; set; }
    public decimal Hours { get; set; }
    public string? Description { get; set; }
    public bool Billable { get; set; } = true;
}
