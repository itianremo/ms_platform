using Microsoft.AspNetCore.SignalR;

namespace Chat.API.Hubs;

using Chat.Domain.Repositories;
using Chat.Domain.Entities;

public interface IChatClient
{
    Task ReceiveMessage(string senderId, string message, string timestamp);
}

public class ChatHub : Hub<IChatClient>
{
    // For simplicity, just use the service directly here, though typically we'd use a Command
    private readonly Application.Common.Interfaces.ITextModerationService _moderationService;
    private readonly IChatRepository _repository;

    public ChatHub(Application.Common.Interfaces.ITextModerationService moderationService, IChatRepository repository)
    {
        _moderationService = moderationService;
        _repository = repository;
    }

    public async Task SendMessageToUser(string userId, string message)
    {
        var appId = Guid.Parse("00000000-0000-0000-0000-000000000000"); // TODO: Get from Context/Claims
        var senderId = Guid.Parse(Context.UserIdentifier ?? Guid.Empty.ToString());
        var recipientId = Guid.Parse(userId);

        if (await _moderationService.ContainsBannedWordsAsync(message))
        {
            // Save as flagged
            var flaggedMsg = new ChatMessage(appId, senderId, recipientId, message) { IsFlagged = true };
            await _repository.AddMessageAsync(flaggedMsg);

            // Notify sender
            await Clients.Caller.ReceiveMessage("System", "Message sent for review (Content Warning).", DateTime.UtcNow.ToString());
            return;
        }

        // Save normal message (optional, but good practice)
        var msg = new ChatMessage(appId, senderId, recipientId, message);
        await _repository.AddMessageAsync(msg);

        await Clients.User(userId).ReceiveMessage(Context.UserIdentifier ?? "Anonymous", message, DateTime.UtcNow.ToString());
    }

    public async Task JoinChannel(string channelId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, channelId);
    }

    public async Task SendMessageToChannel(string channelId, string message)
    {
         // Verify user is member of channel using Repository...
         // For now, just broadcast to group
         await Clients.Group(channelId).ReceiveMessage(Context.UserIdentifier ?? "Anonymous", message, DateTime.UtcNow.ToString());
    }
}
