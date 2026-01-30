using Shared.Kernel;

namespace Notifications.Domain.Entities;

public class UserNotification : Entity
{
    public Guid UserId { get; private set; }
    public string Title { get; private set; }
    public string Message { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? Link { get; private set; }

    private UserNotification() { }

    public UserNotification(Guid userId, string title, string message, string? link = null)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Title = title;
        Message = message;
        Link = link;
        IsRead = false;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsRead()
    {
        IsRead = true;
    }
}
