using Shared.Kernel;

namespace Auth.Domain.Entities;

public enum AppUserStatus
{
    Active,
    Banned,
    PendingApproval
}

public class UserAppMembership : Entity
{
    public Guid UserId { get; private set; }
    public Guid AppId { get; private set; }
    public Guid RoleId { get; private set; } // Link to Role
    public virtual Role Role { get; private set; } // Navigation Property
    public AppUserStatus Status { get; private set; }
    public string SettingsJson { get; private set; } // Flexible settings per app membership

    private UserAppMembership() { }

    public UserAppMembership(Guid userId, Guid appId, Guid roleId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        AppId = appId;
        RoleId = roleId;
        Status = AppUserStatus.Active;
        SettingsJson = "{}";
    }

    public void ChangeRole(Guid newRoleId)
    {
        RoleId = newRoleId;
    }

    public void Ban() => Status = AppUserStatus.Banned;
    public void Activate() => Status = AppUserStatus.Active;
    public void SetStatus(AppUserStatus status) => Status = status;
}
