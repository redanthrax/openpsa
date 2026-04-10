using Common.Domain;

namespace OpenPsa.Modules.TimeEntries.Models;

public class TimeEntry : BaseEntity {
    public Guid ClientId { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid? TicketId { get; set; }
    public Guid UserId { get; set; }
    public DateTime Date { get; set; }
    public decimal Hours { get; set; }
    public string? Description { get; set; }
    public bool Billable { get; set; } = true;
    public bool Invoiced { get; set; }
}
