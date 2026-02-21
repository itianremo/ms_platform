using System;
using Shared.Kernel;

namespace Users.Domain.Entities;

public class UserReport : Entity
{
    public Guid AppId { get; set; }
    public Guid ReporterId { get; set; }
    public Guid ReportedId { get; set; }
    public string ReasonText { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Pending";

    public UserReport()
    {
        Id = Guid.NewGuid();
    }
}
