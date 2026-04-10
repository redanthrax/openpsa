using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Common.Notifications;

public class NotificationService : INotificationService {
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IHubContext<NotificationHub> hubContext, ILogger<NotificationService> logger) {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendToAllAsync(string method, object payload, CancellationToken cancellationToken = default) {
        _logger.LogDebug("Sending notification {Method} to all clients", method);
        await _hubContext.Clients.All.SendAsync(method, payload, cancellationToken).ConfigureAwait(false);
    }

    public async Task SendToUserAsync(string userId, string method, object payload, CancellationToken cancellationToken = default) {
        _logger.LogDebug("Sending notification {Method} to user {UserId}", method, userId);
        await _hubContext.Clients.User(userId).SendAsync(method, payload, cancellationToken).ConfigureAwait(false);
    }
}
