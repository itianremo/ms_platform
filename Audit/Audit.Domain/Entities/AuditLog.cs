using Shared.Kernel;
using System.ComponentModel.DataAnnotations;

namespace Audit.Domain.Entities;

public class AuditLog : Entity
{
    [Required]
    public string Action { get; set; } = string.Empty; // e.g. "UserParamsUpdated"

    [Required]
    public string EntityName { get; set; } = string.Empty; // e.g. "UserProfile"

    public string? EntityId { get; set; } // ID of the entity changed

    public Guid? UserId { get; set; } // Who did it

    public Guid? AppId { get; set; } // In which tenant

    public string ChangesJson { get; set; } = "{}"; // Serialized diff or payload

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public AuditLog() { }

    public AuditLog(string action, string entityName, string entityId, Guid? userId, Guid? appId, string changesJson)
    {
        Action = action;
        EntityName = entityName;
        EntityId = entityId;
        UserId = userId;
        AppId = appId;
        ChangesJson = changesJson;
        Timestamp = DateTime.UtcNow;
    }
}
