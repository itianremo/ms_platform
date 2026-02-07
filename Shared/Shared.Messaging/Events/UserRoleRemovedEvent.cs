namespace Shared.Messaging.Events;

public record UserRoleRemovedEvent(Guid UserId, Guid AppId, Guid RoleId, string RoleName);
