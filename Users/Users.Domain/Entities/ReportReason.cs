using System;
using Shared.Kernel;

namespace Users.Domain.Entities;

public class ReportReason : Entity
{
    public Guid AppId { get; set; }
    public string ReasonText { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ReportReason()
    {
        Id = Guid.NewGuid();
    }
}
