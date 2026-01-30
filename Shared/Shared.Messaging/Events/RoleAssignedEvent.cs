namespace Shared.Messaging.Events;

public record RoleAssignedEvent(
    Guid UserId, 
    Guid AppId, 
    Guid RoleId, 
    string RoleName, 
    string OldRoleName, 
    Guid? PerformedByUserId, 
    DateTime Timestamp
);
