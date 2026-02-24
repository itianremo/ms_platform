using MediatR;
using Users.Domain.Repositories;

namespace Users.Application.Features.Users.Queries.GetAuthProfile;

public class GetAuthProfileQueryHandler : IRequestHandler<GetAuthProfileQuery, AuthProfileDto?>
{
    private readonly IUserProfileRepository _profileRepository;

    public GetAuthProfileQueryHandler(IUserProfileRepository profileRepository)
    {
        _profileRepository = profileRepository;
    }

    public async Task<AuthProfileDto?> Handle(GetAuthProfileQuery request, CancellationToken cancellationToken)
    {
        var profile = await _profileRepository.GetByUserIdAndAppIdAsync(request.UserId, request.AppId);
        if (profile == null) return null;

        return new AuthProfileDto
        {
            RoleId = profile.RoleId,
            Status = profile.Status
        };
    }
}
