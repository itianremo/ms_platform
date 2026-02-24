using Shared.Kernel;

namespace Shared.Messaging.Events;

public class UserRegisteredEvent : IDomainEvent
{
    public Guid UserId { get; set; }
    public Guid AppId { get; set; }
    public string Email { get; set; }
    public string DisplayName { get; set; }
    public string Phone { get; set; }
    public string? InitialPassword { get; set; }
    public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
    public Guid? RoleId { get; set; }

    public UserRegisteredEvent(Guid userId, Guid appId, string email, string phone, string displayName, string? initialPassword = null, Guid? roleId = null)
    {
        UserId = userId;
        AppId = appId;
        Email = email;
        Phone = phone;
        DisplayName = displayName;
        InitialPassword = initialPassword;
        RoleId = roleId;
        OccurredOn = DateTime.UtcNow;
    }
}
