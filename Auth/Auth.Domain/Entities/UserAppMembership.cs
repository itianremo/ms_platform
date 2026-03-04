using Shared.Kernel;

namespace Auth.Domain.Entities;

public class UserAppMembership : Entity
{
    public Guid UserId { get; private set; }
    public Guid AppId { get; private set; }
    public Guid RoleId { get; private set; }
    
    public string Status { get; private set; } = "Active";
    
    // New property replacing SubscriptionExpiry
    public Guid? UserSubscriptionId { get; private set; }

    // Removed SettingsJson

    public DateTime? LastLoginUtc { get; private set; }

    // Navigation properties
    public User User { get; private set; } = null!;
    public Role Role { get; private set; } = null!;

    private UserAppMembership() { }

    public UserAppMembership(Guid userId, Guid appId, Guid roleId, Guid? userSubscriptionId = null)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        AppId = appId;
        RoleId = roleId;
        UserSubscriptionId = userSubscriptionId;
        Status = "Active";
    }

    public void UpdateSubscription(Guid? userSubscriptionId)
    {
        UserSubscriptionId = userSubscriptionId;
    }

    public void RecordLogin()
    {
        LastLoginUtc = DateTime.UtcNow;
    }
}
