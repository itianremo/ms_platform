using System.Text.Json;
using System.Text.Json.Serialization;
using Users.Domain.Enums;

namespace Users.Application.DTOs;

public class UserProfileDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid AppId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    
    public Guid RoleId { get; set; }
    public AppUserStatus Status { get; set; }
    public int LoyaltyPoints { get; set; }
    public int CoinsBalance { get; set; }
    
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastActiveAt { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? DynamicData { get; set; }
}
