namespace Contracts.Email;

public class CreateMailboxConnectionRequest {
    public string Name { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public MailboxProvider Provider { get; set; }
    public Guid? DefaultQueueId { get; set; }
    public string? ImapHost { get; set; }
    public int? ImapPort { get; set; }
    public bool ImapUseSsl { get; set; } = true;
    public string? ImapUsername { get; set; }
    public string? ImapPassword { get; set; }
    public string? SmtpHost { get; set; }
    public int? SmtpPort { get; set; }
    public bool SmtpUseSsl { get; set; } = true;
    public string? SmtpUsername { get; set; }
    public string? SmtpPassword { get; set; }
    public string? GraphTenantId { get; set; }
    public string? GraphClientId { get; set; }
    public string? GraphClientSecret { get; set; }
    public string? GraphMailboxUserId { get; set; }
    public int PollIntervalSeconds { get; set; } = 60;
    public bool AutoCreateContacts { get; set; } = true;
}
