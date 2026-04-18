using Azure.Identity;
using Common.Security;
using Contracts.Email;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.Messages.Delta;
using Microsoft.Graph.Users.Item.SendMail;
using OpenPsa.Modules.Email.Models;

namespace OpenPsa.Modules.Email.Services;

public class GraphMailService {
    private readonly IPiiEncryptionService _pii;
    private readonly ILogger<GraphMailService> _logger;

    public GraphMailService(IPiiEncryptionService pii, ILogger<GraphMailService> logger) {
        _pii = pii;
        _logger = logger;
    }

    public GraphServiceClient CreateClient(MailboxConnection mailbox) {
        if (string.IsNullOrEmpty(mailbox.GraphTenantId) ||
            string.IsNullOrEmpty(mailbox.GraphClientId) ||
            string.IsNullOrEmpty(mailbox.EncryptedGraphClientSecret))
            throw new InvalidOperationException("Graph credentials not configured on mailbox connection");

        var clientSecret = _pii.Decrypt(mailbox.EncryptedGraphClientSecret);

        var credential = new ClientSecretCredential(
            mailbox.GraphTenantId,
            mailbox.GraphClientId,
            clientSecret);

        return new GraphServiceClient(credential, ["https://graph.microsoft.com/.default"]);
    }

    public async Task<(List<EmailMessage> Messages, string? DeltaLink)> PollMessagesAsync(
        MailboxConnection mailbox, CancellationToken ct) {
        var client = CreateClient(mailbox);
        var userId = mailbox.GraphMailboxUserId ?? mailbox.EmailAddress;
        var results = new List<EmailMessage>();

        DeltaGetResponse? page;

        if (!string.IsNullOrEmpty(mailbox.GraphDeltaLink)) {
            page = await client.Users[userId].Messages.Delta
                .WithUrl(mailbox.GraphDeltaLink)
                .GetAsDeltaGetResponseAsync(cancellationToken: ct);
        } else {
            page = await client.Users[userId].Messages.Delta
                .GetAsDeltaGetResponseAsync(r => {
                    r.QueryParameters.Top = 50;
                    r.QueryParameters.Select = [
                        "id", "subject", "from", "toRecipients", "body",
                        "receivedDateTime", "internetMessageId", "internetMessageHeaders",
                        "hasAttachments", "isRead"
                    ];
                    r.QueryParameters.Filter = "isRead eq false";
                }, ct);
        }

        string? deltaLink = null;

        while (page != null) {
            if (page.Value != null) {
                foreach (var msg in page.Value) {
                    var emailMsg = MapGraphMessageToEmail(msg, mailbox.Id);
                    if (emailMsg != null)
                        results.Add(emailMsg);

                    try {
                        await client.Users[userId].Messages[msg.Id]
                            .PatchAsync(new Message { IsRead = true }, cancellationToken: ct);
                    } catch (Exception ex) {
                        _logger.LogWarning(ex, "Failed to mark message {MessageId} as read", msg.Id);
                    }
                }
            }

            if (page.OdataNextLink != null) {
                page = await client.Users[userId].Messages.Delta
                    .WithUrl(page.OdataNextLink)
                    .GetAsDeltaGetResponseAsync(cancellationToken: ct);
            } else {
                deltaLink = page.OdataDeltaLink;
                break;
            }
        }

        return (results, deltaLink);
    }

    public async Task SendAsync(MailboxConnection mailbox, SendTicketEmailRequest request,
        string? inReplyTo, string? references, CancellationToken ct) {
        var client = CreateClient(mailbox);
        var userId = mailbox.GraphMailboxUserId ?? mailbox.EmailAddress;

        var message = new Message {
            Subject = request.Subject,
            Body = new ItemBody {
                ContentType = BodyType.Html,
                Content = request.BodyHtml
            },
            ToRecipients = [
                new Recipient {
                    EmailAddress = new Microsoft.Graph.Models.EmailAddress {
                        Address = request.ToAddress
                    }
                }
            ]
        };

        if (!string.IsNullOrEmpty(inReplyTo) || !string.IsNullOrEmpty(references)) {
            message.InternetMessageHeaders ??= [];
            if (!string.IsNullOrEmpty(inReplyTo))
                message.InternetMessageHeaders.Add(new InternetMessageHeader { Name = "In-Reply-To", Value = inReplyTo });
            if (!string.IsNullOrEmpty(references))
                message.InternetMessageHeaders.Add(new InternetMessageHeader { Name = "References", Value = references });
        }

        await client.Users[userId].SendMail.PostAsync(new SendMailPostRequestBody {
            Message = message,
            SaveToSentItems = true
        }, cancellationToken: ct);
    }

    public async Task<TestMailboxConnectionResult> TestConnectionAsync(MailboxConnection mailbox, CancellationToken ct) {
        var client = CreateClient(mailbox);
        var userId = mailbox.GraphMailboxUserId ?? mailbox.EmailAddress;

        var inbox = await client.Users[userId].MailFolders["inbox"]
            .GetAsync(r => {
                r.QueryParameters.Select = ["displayName", "totalItemCount", "unreadItemCount"];
            }, ct);

        return new TestMailboxConnectionResult {
            Success = true,
            Message = "Graph API connection successful",
            MailboxInfo = $"{inbox?.DisplayName}: {inbox?.TotalItemCount} messages ({inbox?.UnreadItemCount} unread)"
        };
    }

    private static EmailMessage? MapGraphMessageToEmail(Message msg, Guid mailboxConnectionId) {
        if (msg.Id == null) return null;

        var fromAddress = msg.From?.EmailAddress?.Address ?? string.Empty;
        var fromName = msg.From?.EmailAddress?.Name ?? string.Empty;
        var toAddress = msg.ToRecipients?.FirstOrDefault()?.EmailAddress?.Address ?? string.Empty;

        string? inReplyTo = null;
        string? references = null;

        if (msg.InternetMessageHeaders != null) {
            foreach (var header in msg.InternetMessageHeaders) {
                if (string.Equals(header.Name, "In-Reply-To", StringComparison.OrdinalIgnoreCase))
                    inReplyTo = header.Value;
                else if (string.Equals(header.Name, "References", StringComparison.OrdinalIgnoreCase))
                    references = header.Value;
            }
        }

        return new EmailMessage {
            MailboxConnectionId = mailboxConnectionId,
            Direction = EmailDirection.Inbound,
            DeliveryStatus = EmailDeliveryStatus.Received,
            FromAddress = fromAddress,
            FromName = fromName,
            ToAddress = toAddress,
            Subject = msg.Subject ?? string.Empty,
            BodyHtml = msg.Body?.ContentType == BodyType.Html ? msg.Body.Content : null,
            BodyText = msg.Body?.ContentType == BodyType.Text ? msg.Body.Content : null,
            MessageId = msg.InternetMessageId,
            InReplyTo = inReplyTo,
            References = references,
            AttachmentCount = msg.HasAttachments == true ? 1 : 0,
            SentAt = msg.ReceivedDateTime?.UtcDateTime ?? DateTime.UtcNow
        };
    }
}
