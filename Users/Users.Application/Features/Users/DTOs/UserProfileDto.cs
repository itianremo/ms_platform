namespace Users.Application.Features.Users.DTOs;

public record UserProfileDto(Guid UserId, string DisplayName, string? AvatarUrl, string? Bio, DateTime? DateOfBirth, string? Gender, string CustomDataJson);
