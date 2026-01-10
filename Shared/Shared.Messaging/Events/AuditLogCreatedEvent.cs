namespace Shared.Messaging.Events;

public record AuditLogCreatedEvent(
    string Action,
    string EntityName,
    string EntityId,
    int? UserId,
    int? AppId,
    string ChangesJson,
    DateTime Timestamp
);
