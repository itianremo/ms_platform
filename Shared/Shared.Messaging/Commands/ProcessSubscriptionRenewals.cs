namespace Shared.Messaging.Commands;

public record ProcessSubscriptionRenewals
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
