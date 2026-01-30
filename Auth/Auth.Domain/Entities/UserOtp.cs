using Shared.Kernel;

namespace Auth.Domain.Entities;

public class UserOtp : Entity
{
    public Guid UserId { get; private set; }
    public string Code { get; private set; }
    public VerificationType Type { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsUsed { get; private set; }
    public int Attempts { get; private set; }

    private UserOtp() { }

    public UserOtp(Guid userId, string code, VerificationType type, DateTime expiresAt)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Code = code;
        Type = type;
        ExpiresAt = expiresAt;
        IsUsed = false;
        Attempts = 0;
    }

    public void MarkAsUsed()
    {
        IsUsed = true;
    }

    public void IncrementAttempts()
    {
        Attempts++;
    }
}
