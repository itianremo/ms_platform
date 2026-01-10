using Chat.Domain.Entities;
using Chat.Domain.Repositories;
using Chat.Infrastructure.Persistence;
using MongoDB.Driver;

namespace Chat.Infrastructure.Repositories;

public class ChatRepository : IChatRepository
{
    private readonly ChatDbContext _context;

    public ChatRepository(ChatDbContext context)
    {
        _context = context;
    }

    public async Task AddMessageAsync(ChatMessage message)
    {
        await _context.Messages.InsertOneAsync(message);
    }

    public async Task<List<ChatMessage>> GetHistoryAsync(Guid senderId, Guid recipientId)
    {
        var filter = Builders<ChatMessage>.Filter.Or(
            Builders<ChatMessage>.Filter.And(
                Builders<ChatMessage>.Filter.Eq(m => m.SenderId, senderId),
                Builders<ChatMessage>.Filter.Eq(m => m.RecipientId, recipientId)
            ),
            Builders<ChatMessage>.Filter.And(
                Builders<ChatMessage>.Filter.Eq(m => m.SenderId, recipientId),
                Builders<ChatMessage>.Filter.Eq(m => m.RecipientId, senderId)
            )
        );

        return await _context.Messages.Find(filter)
            .SortBy(m => m.Timestamp)
            .ToListAsync();
    }

    public async Task<List<ChatMessage>> GetUnreadAsync(Guid recipientId)
    {
        var filter = Builders<ChatMessage>.Filter.And(
            Builders<ChatMessage>.Filter.Eq(m => m.RecipientId, recipientId),
            Builders<ChatMessage>.Filter.Eq(m => m.IsRead, false)
        );

        return await _context.Messages.Find(filter).ToListAsync();
    }

    public async Task<List<ChatMessage>> GetFlaggedAsync(Guid appId)
    {
        var filter = Builders<ChatMessage>.Filter.And(
            Builders<ChatMessage>.Filter.Eq(m => m.AppId, appId),
            Builders<ChatMessage>.Filter.Eq(m => m.IsFlagged, true)
        );

        return await _context.Messages.Find(filter).SortByDescending(m => m.Timestamp).ToListAsync();
    }
}
