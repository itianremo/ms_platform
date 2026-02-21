namespace Recommendation.Application.DTOs;

public record UserProfileDto(Guid UserId, string DisplayName, string? AvatarUrl, string? Bio, DateTime? DateOfBirth, string? Gender, List<string> Images, string? City, string? Country, string? CustomDataJson);
