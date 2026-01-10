namespace Chat.Domain.Entities;

public class ChatMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AppId { get; set; } // Tenant
    public Guid SenderId { get; set; }
    public Guid RecipientId { get; set; } // Can be UserID or GroupID
    public string Content { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;
    public bool IsFlagged { get; set; } = false;

    public ChatMessage(Guid appId, Guid senderId, Guid recipientId, string content)
    {
        AppId = appId;
        SenderId = senderId;
        RecipientId = recipientId;
        Content = content;
    }
}
