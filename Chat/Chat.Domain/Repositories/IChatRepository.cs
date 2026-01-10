using Chat.Domain.Entities;

namespace Chat.Domain.Repositories;

public interface IChatRepository
{
    Task AddMessageAsync(ChatMessage message);
    Task<List<ChatMessage>> GetHistoryAsync(Guid senderId, Guid recipientId);
    Task<List<ChatMessage>> GetUnreadAsync(Guid recipientId);
    Task<List<ChatMessage>> GetFlaggedAsync(Guid appId);
}
