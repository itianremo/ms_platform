namespace Shared.Messaging.Events;

public class SubscriptionGrantedEvent
{
    public Guid UserId { get; set; }
    public Guid AppId { get; set; }
    public Guid PackageId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
}
