using MediatR;
using Users.Domain.Repositories;
using Users.Application.Features.Users.DTOs;

namespace Users.Application.Features.Users.Queries.GetProfiles;

public class GetProfilesQueryHandler : IRequestHandler<GetProfilesQuery, List<UserProfileDto>>
{
    private readonly IUserProfileRepository _repository;

    public GetProfilesQueryHandler(IUserProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<UserProfileDto>> Handle(GetProfilesQuery request, CancellationToken cancellationToken)
    {
        // For simplicity, we just return profiles for the requested AppId.
        // If AppId is not provided (e.g. System Admin?), we might want logic to return all.
        // But the query enforces AppId.
        
        var profiles = await _repository.GetAllProfilesAsync(request.AppId);
        
        return profiles.Select(p => new UserProfileDto(
            p.UserId,
            GetEffectiveDisplayName(p.DisplayName),
            p.AvatarUrl,
            p.Bio,
            p.DateOfBirth,
            p.Gender,
            p.CustomDataJson
        )).ToList();
    }

    private static string GetEffectiveDisplayName(string? displayName)
    {
        if (!string.IsNullOrWhiteSpace(displayName) && displayName != "N/A" && displayName != "n/a" && displayName != "Unknown")
            return displayName;
        
        return "Unknown User";
    }
}
