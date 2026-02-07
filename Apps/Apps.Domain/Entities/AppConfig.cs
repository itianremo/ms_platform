using Shared.Kernel;

namespace Apps.Domain.Entities;



public class AppConfig : Entity
{
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public string BaseUrl { get; private set; } = null!;
    public string ThemeJson { get; private set; } = "{}";
    public string DefaultUserProfileJson { get; private set; } = "{}";
    public string ExternalAuthProvidersJson { get; private set; } = "{}";
    
    public bool IsActive { get; private set; }
    public VerificationType VerificationType { get; private set; }
    public bool RequiresAdminApproval { get; private set; }

    public void UpdateVerificationType(VerificationType type)
    {
        VerificationType = type;
    }

    public void UpdateRequirements(bool requiresAdminApproval)
    {
        RequiresAdminApproval = requiresAdminApproval;
    }

    private AppConfig() { }

    public AppConfig(string name, string description, string baseUrl, Guid? id = null)
    {
        Id = id ?? Guid.NewGuid();
        Name = name;
        Description = description;
        BaseUrl = baseUrl;
        IsActive = true;
        VerificationType = VerificationType.None; // Default exception
        RequiresAdminApproval = false;

        ThemeJson = "{}";
        DefaultUserProfileJson = "{}";
        ExternalAuthProvidersJson = "{}";
    }

    public void UpdateTheme(string themeJson)
    {
        ThemeJson = themeJson;
    }

    public void UpdateDefaultUserProfile(string json)
    {
        DefaultUserProfileJson = json;
    }

    public void UpdateExternalAuthProviders(string json)
    {
        ExternalAuthProvidersJson = json;
    }

    public void UpdateDetails(string name, string description, string baseUrl)
    {
        Name = name;
        Description = description;
        BaseUrl = baseUrl;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
