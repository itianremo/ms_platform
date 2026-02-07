namespace Shared.Messaging.Events;

public record UserLockedOutEvent(Guid UserId, string Email, DateTime LockoutEnd, string IpAddress);
