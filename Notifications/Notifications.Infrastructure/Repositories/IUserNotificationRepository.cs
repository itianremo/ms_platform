using Notifications.Domain.Entities;

namespace Notifications.Infrastructure.Repositories;

public interface IUserNotificationRepository
{
    Task<List<UserNotification>> GetUnreadByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<List<UserNotification>> GetAllByUserIdAsync(Guid userId, int count, CancellationToken cancellationToken);
    Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken);
    Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken);
    Task AddAsync(UserNotification notification, CancellationToken cancellationToken);
}
