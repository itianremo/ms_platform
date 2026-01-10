using Shared.Kernel;

namespace Apps.Domain.Entities;

public class AppConfig : Entity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string BaseUrl { get; private set; }
    public string ThemeJson { get; private set; } // For storing UI theme settings
    public bool IsActive { get; private set; }

    private AppConfig() { }

    public AppConfig(string name, string description, string baseUrl)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        BaseUrl = baseUrl;
        IsActive = true;
        ThemeJson = "{}";
    }

    public void UpdateTheme(string themeJson)
    {
        ThemeJson = themeJson;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
