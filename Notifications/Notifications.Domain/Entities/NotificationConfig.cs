using Shared.Kernel;

namespace Notifications.Domain.Entities;

public class NotificationConfig : Entity
{
    public string Type { get; set; } = default!; // "Email" or "Sms"
    public string Provider { get; set; } = default!; // "SMTP", "SendGrid", "Twilio", etc.
    public string ConfigJson { get; set; } = default!; // Serialized JSON config
    public bool IsActive { get; set; }
    public string? TenantId { get; set; } // Null for global default
}
