namespace Contracts.Email;

public class EmailMessageSummaryDto {
    public Guid Id { get; set; }
    public Guid? TicketId { get; set; }
    public string? TicketNumber { get; set; }
    public EmailDirection Direction { get; set; }
    public EmailDeliveryStatus DeliveryStatus { get; set; }
    public string FromAddress { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public int AttachmentCount { get; set; }
    public DateTime SentAt { get; set; }
}
