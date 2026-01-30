using Shared.Kernel;

namespace Auth.Domain.Entities;

public class UserSession : Entity
{
    public Guid UserId { get; private set; }
    public Guid? AppId { get; private set; } // Optional: Session bound to specific app?
    public string RefreshToken { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public string? DeviceInfo { get; private set; } // Processed user agent (e.g. "Chrome on Windows")
    public bool IsRevoked { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public virtual User User { get; private set; }

    private UserSession() { }

    public UserSession(Guid userId, Guid? appId, string refreshToken, DateTime expiresAt, string? ipAddress, string? userAgent)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        AppId = appId;
        RefreshToken = refreshToken;
        ExpiresAt = expiresAt;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        CreatedAt = DateTime.UtcNow;
        IsRevoked = false;
        
        // Simple device parsing logic could go here or in handler
        DeviceInfo = ParseUserAgent(userAgent);
    }

    public void Revoke()
    {
        IsRevoked = true;
    }

    private string ParseUserAgent(string? ua)
    {
        if (string.IsNullOrEmpty(ua)) return "Unknown Device";
        // Simple fallback
        if (ua.Contains("Windows")) return "Windows PC";
        if (ua.Contains("Mac")) return "Mac";
        if (ua.Contains("iPhone")) return "iPhone";
        if (ua.Contains("Android")) return "Android";
        return "Unknown Device";
    }
}
