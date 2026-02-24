using Shared.Kernel;
using Users.Domain.Enums;

namespace Users.Domain.Entities;

public class UserProfile : Entity
{
    public Guid UserId { get; private set; } // Global Identity ID
    public Guid AppId { get; private set; } // Scoped to App
    public string DisplayName { get; private set; }

    public string? AvatarUrl { get; private set; }
    public string? Bio { get; private set; }
    

    // JSON blob for app-specific data (e.g. "Height", "Certifications")
    public string? CustomDataJson { get; private set; } 

    // App Membership details
    public Guid RoleId { get; private set; }
    public AppUserStatus Status { get; private set; }
    public string SettingsJson { get; private set; }
    public DateTime? SubscriptionExpiry { get; private set; }

    // Additional standard fields
    public DateTime? DateOfBirth { get; private set; }
    public string? Gender { get; private set; }
    public DateTime Created { get; private set; } = DateTime.UtcNow;
    public DateTime? LastActiveAt { get; set; } // Settable for activity tracking

    private UserProfile() { 
        DisplayName = "";
        SettingsJson = "{}";
    }

    public UserProfile(Guid userId, Guid appId, string displayName, string? customDataJson = "{}", Guid? roleId = null)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        AppId = appId;
        DisplayName = displayName;
        CustomDataJson = customDataJson;
        
        RoleId = roleId ?? Guid.Empty; // Temporary default to Empty if not provided initially
        Status = AppUserStatus.Active;
        SettingsJson = "{}";
    }

    public void UpdateProfile(string displayName, string? bio, string? avatarUrl, string? customDataJson, DateTime? dateOfBirth, string? gender)
    {
        DisplayName = displayName;
        Bio = bio;
        AvatarUrl = avatarUrl;
        CustomDataJson = customDataJson;
        DateOfBirth = dateOfBirth;
        Gender = gender;
    }

    public void ChangeRole(Guid newRoleId) => RoleId = newRoleId;
    public void Ban() => Status = AppUserStatus.Banned;
    public void Activate() => Status = AppUserStatus.Active;
    public void SetStatus(AppUserStatus status) => Status = status;
    public void UpdateSubscriptionExpiry(DateTime? expiry) => SubscriptionExpiry = expiry;
    public void UpdateSettings(string settingsJson) => SettingsJson = settingsJson;
}
