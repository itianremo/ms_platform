using Shared.Kernel;

namespace Auth.Domain.Entities;

public class UserLogin : Entity
{
    public Guid UserId { get; private set; }
    public string LoginProvider { get; private set; } // e.g. "Google", "Facebook"
    public string ProviderKey { get; private set; } // Unique user ID from the provider
    public string? ProviderDisplayName { get; private set; } // e.g. "John Doe"

    public virtual User User { get; private set; }

    private UserLogin() { }

    public UserLogin(Guid userId, string loginProvider, string providerKey, string? providerDisplayName)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        LoginProvider = loginProvider;
        ProviderKey = providerKey;
        ProviderDisplayName = providerDisplayName;
    }
}
