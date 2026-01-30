using Notifications.Domain.Entities;

namespace Notifications.Application.Common.Interfaces;

public interface INotificationConfigRepository
{
    Task<NotificationConfig?> GetByTypeAsync(string type, CancellationToken cancellationToken);
    Task AddAsync(NotificationConfig config, CancellationToken cancellationToken);
    Task UpdateAsync(NotificationConfig config, CancellationToken cancellationToken);
    Task<List<NotificationConfig>> GetAllAsync(CancellationToken cancellationToken);
}
