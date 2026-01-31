using Microsoft.AspNetCore.SignalR;
using Notifications.API.Hubs;
using Notifications.Application.Common.Interfaces;

namespace Notifications.API.Services;

public class SignalRService : ISignalRService
{
    private readonly IHubContext<NotificationHub, INotificationClient> _hubContext;

    public SignalRService(IHubContext<NotificationHub, INotificationClient> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendToUserAsync(string userId, string title, string message)
    {
        await _hubContext.Clients.User(userId).ReceiveNotification(title, message);
    }

    public async Task BroadcastAsync(string title, string message)
    {
        await _hubContext.Clients.All.ReceiveNotification(title, message);
    }
}
