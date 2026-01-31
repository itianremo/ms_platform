using System.Threading.Tasks;

namespace Notifications.Application.Common.Interfaces;

public interface ISignalRService
{
    Task SendToUserAsync(string userId, string title, string message);
    Task BroadcastAsync(string title, string message);
}
