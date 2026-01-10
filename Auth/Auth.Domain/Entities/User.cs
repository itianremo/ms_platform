using Shared.Kernel;

namespace Auth.Domain.Entities;

public enum GlobalUserStatus
{
    PendingApproval,
    Active,
    Banned,
    SoftDeleted
}

public class User : Entity
{
    public string Email { get; private set; }
    public string Phone { get; private set; }
    public string PasswordHash { get; private set; }
    public bool IsGlobalAdmin { get; private set; }
    public bool IsSealed { get; private set; } // Cannot be deleted or modified
    public GlobalUserStatus Status { get; private set; }

    private readonly List<UserAppMembership> _memberships = new();
    public IReadOnlyCollection<UserAppMembership> Memberships => _memberships.AsReadOnly();

    private readonly List<UserLogin> _logins = new();
    public IReadOnlyCollection<UserLogin> Logins => _logins.AsReadOnly();

    private User() { }

    public User(string email, string phone, string passwordHash, bool isGlobalAdmin = false)
    {
        Id = Guid.NewGuid();
        Email = email;
        Phone = phone;
        PasswordHash = passwordHash;
        IsGlobalAdmin = isGlobalAdmin;
        Status = GlobalUserStatus.PendingApproval;
    }

    public void Activate()
    {
        Status = GlobalUserStatus.Active;
    }

    public void Ban()
    {
        Status = GlobalUserStatus.Banned;
    }

    public void SoftDelete()
    {
        Status = GlobalUserStatus.SoftDeleted;
    }

    public void AddMembership(UserAppMembership membership)
    {
        // Validation logic can go here (e.g. check duplicate app)
        _memberships.Add(membership);
    }

    public void AddLogin(UserLogin login)
    {
        if (!_logins.Any(l => l.LoginProvider == login.LoginProvider && l.ProviderKey == login.ProviderKey))
        {
            _logins.Add(login);
        }
    }

    public void MarkAsSealed()
    {
        IsSealed = true;
    }
}
