using MediatR;

namespace Users.Application.Features.Users.Commands.UpdateProfile;

public record UpdateProfileCommand(
    Guid UserId, 
    Guid AppId, 
    string DisplayName, 
    string? Bio, 
    string? AvatarUrl, 
    string CustomDataJson,
    DateTime? DateOfBirth,
    string? Gender
) : IRequest;
