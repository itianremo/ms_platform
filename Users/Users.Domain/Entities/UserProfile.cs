using Shared.Kernel;

namespace Users.Domain.Entities;

public class UserProfile : Entity
{
    public Guid UserId { get; private set; } // Global Identity ID
    public Guid AppId { get; private set; } // Scoped to App
    public string DisplayName { get; private set; }
    public string? AvatarUrl { get; private set; }
    public string? Bio { get; private set; }
    


    // JSON blob for app-specific data (e.g. "Height", "Certifications")
    public string CustomDataJson { get; private set; } 

    // Additional standard fields
    public DateTime? DateOfBirth { get; private set; }
    public string? Gender { get; private set; }
    public DateTime Created { get; private set; } = DateTime.UtcNow;

    private UserProfile() { }

    public UserProfile(Guid userId, Guid appId, string displayName, string customDataJson = "{}")
    {
        Id = Guid.NewGuid();
        UserId = userId;
        AppId = appId;
        DisplayName = displayName;
        CustomDataJson = customDataJson;
    }

    public void UpdateProfile(string displayName, string? bio, string? avatarUrl, string customDataJson, DateTime? dateOfBirth, string? gender)
    {
        DisplayName = displayName;
        Bio = bio;
        AvatarUrl = avatarUrl;
        CustomDataJson = customDataJson;
        DateOfBirth = dateOfBirth;
        Gender = gender;
    }
}
