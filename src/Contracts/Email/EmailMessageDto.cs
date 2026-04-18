namespace Contracts.Email;

public class EmailMessageDto {
    public Guid Id { get; set; }
    public Guid MailboxConnectionId { get; set; }
    public Guid? TicketId { get; set; }
    public Guid? ContactId { get; set; }
    public Guid? ClientId { get; set; }
    public EmailDirection Direction { get; set; }
    public EmailDeliveryStatus DeliveryStatus { get; set; }
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string ToAddress { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? BodyHtml { get; set; }
    public string? BodyText { get; set; }
    public string? MessageId { get; set; }
    public string? InReplyTo { get; set; }
    public string? References { get; set; }
    public int AttachmentCount { get; set; }
    public string? ErrorDetails { get; set; }
    public DateTime SentAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
