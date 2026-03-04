using MediatR;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Users.Application.Features.Users.Commands.UpdateProfile;

public class UpdateProfileCommand : IRequest
{
    public Guid UserId { get; set; }
    public Guid AppId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? DynamicData { get; set; }
}
