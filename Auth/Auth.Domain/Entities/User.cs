using Shared.Kernel;

namespace Auth.Domain.Entities;

public enum GlobalUserStatus
{
    PendingAccountVerification, // 0
    PendingMobileVerification,  // 1
    PendingEmailVerification,   // 2
    PendingAdminApproval,       // 3
    Active,                     // 4
    Banned,                     // 5
    SoftDeleted,                // 6
    ProfileIncomplete           // 7
}

public class User : Entity
{
    public string Email { get; private set; } = null!;
    public string Phone { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;

    // Removed IsGlobalAdmin
    public bool IsSealed { get; private set; } // Cannot be deleted or modified
    public GlobalUserStatus Status { get; private set; }
    public DateTime? OtpBlockedUntil { get; private set; }


    private readonly List<UserLogin> _logins = new();
    public IReadOnlyCollection<UserLogin> Logins => _logins.AsReadOnly();

    private readonly List<UserSession> _sessions = new();
    public IReadOnlyCollection<UserSession> Sessions => _sessions.AsReadOnly();

    public DateTime? LastLoginUtc { get; private set; }
    public Guid? LastLoginAppId { get; private set; }

    private User() { }

    public User(string email, string phone, string passwordHash)
    {
        Id = Guid.NewGuid();
        Email = email;
        Phone = phone;
        PasswordHash = passwordHash;
        Status = GlobalUserStatus.PendingAccountVerification;
    }

    public User(Guid id, string email, string phone, string passwordHash)
    {
        Id = id;
        Email = email;
        Phone = phone;
        PasswordHash = passwordHash;
        Status = GlobalUserStatus.PendingAccountVerification;
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

    public bool IsEmailVerified { get; private set; }
    public bool IsPhoneVerified { get; private set; }

    public void VerifyEmail()
    {
        IsEmailVerified = true;
    }

    public void VerifyPhone()
    {
        IsPhoneVerified = true;
    }

    public void SetStatus(GlobalUserStatus status)
    {
        Status = status;
    }

    public void UpdateEmail(string newEmail)
    {
        Email = newEmail;
        IsEmailVerified = false;
    }

    public void UpdatePhone(string newPhone)
    {
        Phone = newPhone;
        IsPhoneVerified = false;
    }

    public void SetEmailVerified(bool verified)
    {
        IsEmailVerified = verified;
    }

    public void SetPhoneVerified(bool verified)
    {
        IsPhoneVerified = verified;
    }

    public void AddLogin(UserLogin login)
    {
        if (!_logins.Any(l => l.LoginProvider == login.LoginProvider && l.ProviderKey == login.ProviderKey))
        {
            _logins.Add(login);
        }
    }

    public void AddSession(UserSession session)
    {
        _sessions.Add(session);
    }

    public void RemoveSession(Guid sessionId)
    {
        var session = _sessions.FirstOrDefault(s => s.Id == sessionId);
        if (session != null)
        {
            _sessions.Remove(session);
        }
    }

    public void ClearExpiredSessions()
    {
        var expiredSessions = _sessions.Where(s => s.ExpiresAt < DateTime.UtcNow).ToList();
        foreach (var session in expiredSessions)
        {
            _sessions.Remove(session);
        }
    }

    public void RemoveLogin(UserLogin login)
    {
        if (_logins.Contains(login))
        {
            _logins.Remove(login);
        }
    }

    public void MarkAsSealed()
    {
        IsSealed = true;
    }

    public void UpdatePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
    }

    public void BlockOtp(DateTime until)
    {
        OtpBlockedUntil = until;
    }

    public void RecordLogin(Guid? appId = null)
    {
        LastLoginUtc = DateTime.UtcNow;
        LastLoginAppId = appId;
        ResetAccessFailedCount(); // Successful login resets the counter
    }

    // Brute Force Protection
    public int AccessFailedCount { get; private set; }
    public DateTime? LockoutEnd { get; private set; }

    public void IncrementAccessFailedCount()
    {
        AccessFailedCount++;
    }

    public void ResetAccessFailedCount()
    {
        AccessFailedCount = 0;
        LockoutEnd = null;
    }

    public void Lockout(DateTime until)
    {
        LockoutEnd = until;
    }
}
