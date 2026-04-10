namespace Common.Notifications;

public interface INotificationService {
    Task SendToAllAsync(string method, object payload, CancellationToken cancellationToken = default);
    Task SendToUserAsync(string userId, string method, object payload, CancellationToken cancellationToken = default);
}
