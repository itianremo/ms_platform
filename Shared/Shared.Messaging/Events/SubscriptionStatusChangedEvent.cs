namespace Shared.Messaging.Events;

public class SubscriptionStatusChangedEvent
{
    public Guid SubscriptionId { get; set; }
    public Guid UserId { get; set; }
    public Guid AppId { get; set; }
    public bool IsActive { get; set; }
    public DateTime? NewExpiry { get; set; }
}
