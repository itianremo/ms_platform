using Microsoft.EntityFrameworkCore;
using Notifications.Domain.Entities;
using Notifications.Infrastructure.Persistence;

namespace Notifications.Infrastructure.Repositories;

public class UserNotificationRepository : IUserNotificationRepository
{
    private readonly NotificationsDbContext _context;

    public UserNotificationRepository(NotificationsDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserNotification>> GetUnreadByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _context.UserNotifications
            .Where(x => x.UserId == userId && !x.IsRead)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<UserNotification>> GetAllByUserIdAsync(Guid userId, int count, CancellationToken cancellationToken)
    {
        return await _context.UserNotifications
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken)
    {
        var notif = await _context.UserNotifications.FindAsync(new object[] { notificationId }, cancellationToken);
        if (notif != null)
        {
            notif.MarkAsRead();
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken)
    {
        var unread = await _context.UserNotifications
            .Where(x => x.UserId == userId && !x.IsRead)
            .ToListAsync(cancellationToken);

        if (unread.Any())
        {
            foreach (var n in unread)
            {
                n.MarkAsRead();
            }
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task AddAsync(UserNotification notification, CancellationToken cancellationToken)
    {
        await _context.UserNotifications.AddAsync(notification, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
