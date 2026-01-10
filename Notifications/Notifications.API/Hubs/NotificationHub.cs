using Microsoft.AspNetCore.SignalR;

namespace Notifications.API.Hubs;

public interface INotificationClient
{
    Task ReceiveMessage(string user, string message);
    Task ReceiveNotification(string title, string content);
}

public class NotificationHub : Hub<INotificationClient>
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.ReceiveMessage(user, message);
    }
    
    public async Task SendNotificationToUser(string userId, string title, string content)
    {
        await Clients.User(userId).ReceiveNotification(title, content);
    }
}
