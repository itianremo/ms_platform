using Shared.Kernel;

namespace Chat.Domain.Entities;

public class ChatChannel : Entity
{
    public string Name { get; set; }
    public List<Guid> MemberIds { get; set; } = new();
    public bool IsPrivate { get; set; } = true;
    
    // For Match, maybe we store MatchId?
    public Guid? RelatedEntityId { get; set; }

    public ChatChannel()
    {
        Id = Guid.NewGuid();
    }
}
