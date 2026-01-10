using Shared.Kernel;
using System.ComponentModel.DataAnnotations;

namespace Search.Domain.Entities;

public class UserSearchProfile : Entity
{
    public Guid UserId { get; set; }
    public Guid AppId { get; set; }

    [MaxLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Bio { get; set; } = string.Empty;

    public int Age { get; set; }

    public string Gender { get; set; } = string.Empty;

    // For simplicity, we can store tags/interests as a JSON array or string for FTS
    public string InterestsJson { get; set; } = "[]"; 

    // Geolocation part (PostGIS later, for now just lat/lon placeholders)
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public DateTime LastActive { get; set; } = DateTime.UtcNow;

    public bool IsVisible { get; set; } = true;
}
