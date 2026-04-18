namespace Contracts.Email;

public class MailboxConnectionDto {
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public MailboxProvider Provider { get; set; }
    public MailboxConnectionStatus Status { get; set; }
    public Guid? DefaultQueueId { get; set; }
    public string? DefaultQueueName { get; set; }
    public string? ImapHost { get; set; }
    public int? ImapPort { get; set; }
    public bool ImapUseSsl { get; set; }
    public string? SmtpHost { get; set; }
    public int? SmtpPort { get; set; }
    public bool SmtpUseSsl { get; set; }
    public string? GraphTenantId { get; set; }
    public string? GraphClientId { get; set; }
    public string? GraphMailboxUserId { get; set; }
    public int PollIntervalSeconds { get; set; } = 60;
    public bool AutoCreateContacts { get; set; }
    public DateTime? LastPollAt { get; set; }
    public int MessageCount { get; set; }
    public string? LastError { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
