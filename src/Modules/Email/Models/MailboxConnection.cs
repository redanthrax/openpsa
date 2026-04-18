using Common.Domain;
using Contracts.Email;

namespace OpenPsa.Modules.Email.Models;

public class MailboxConnection : BaseEntity {
    public string Name { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public MailboxProvider Provider { get; set; }
    public MailboxConnectionStatus Status { get; set; } = MailboxConnectionStatus.Active;
    public Guid? DefaultQueueId { get; set; }
    public string? ImapHost { get; set; }
    public int? ImapPort { get; set; }
    public bool ImapUseSsl { get; set; }
    public string? ImapUsername { get; set; }
    public string? EncryptedImapPassword { get; set; }
    public string? SmtpHost { get; set; }
    public int? SmtpPort { get; set; }
    public bool SmtpUseSsl { get; set; }
    public string? SmtpUsername { get; set; }
    public string? EncryptedSmtpPassword { get; set; }
    public string? GraphTenantId { get; set; }
    public string? GraphClientId { get; set; }
    public string? EncryptedGraphClientSecret { get; set; }
    public string? GraphMailboxUserId { get; set; }
    public string? GraphDeltaLink { get; set; }
    public string? GraphSubscriptionId { get; set; }
    public DateTime? GraphSubscriptionExpiresAt { get; set; }
    public int PollIntervalSeconds { get; set; } = 60;
    public bool AutoCreateContacts { get; set; } = true;
    public DateTime? LastPollAt { get; set; }
    public string? LastSyncUid { get; set; }
    public int MessageCount { get; set; }
    public string? LastError { get; set; }
    public List<EmailMessage> Messages { get; set; } = [];
}
