namespace Contracts.Email;

public class SendTicketEmailRequest {
    public Guid TicketId { get; set; }
    public string ToAddress { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string BodyHtml { get; set; } = string.Empty;
}
