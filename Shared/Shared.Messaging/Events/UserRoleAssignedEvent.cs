namespace Shared.Messaging.Events;

public record UserRoleAssignedEvent(Guid UserId, Guid AppId, Guid RoleId, string RoleName);
