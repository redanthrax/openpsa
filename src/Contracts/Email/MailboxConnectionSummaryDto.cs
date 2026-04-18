namespace Contracts.Email;

public class MailboxConnectionSummaryDto {
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public MailboxProvider Provider { get; set; }
    public MailboxConnectionStatus Status { get; set; }
    public string? DefaultQueueName { get; set; }
    public DateTime? LastPollAt { get; set; }
    public int MessageCount { get; set; }
    public string? LastError { get; set; }
}
