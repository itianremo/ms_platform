namespace Shared.Messaging.Events;

public record AuditLogCreatedEvent(
    string Action,
    string EntityName,
    string EntityId,
    Guid? UserId,
    Guid? AppId,
    string ChangesJson,
    string? IpAddress,
    string? UserAgent,
    DateTime Timestamp
);
