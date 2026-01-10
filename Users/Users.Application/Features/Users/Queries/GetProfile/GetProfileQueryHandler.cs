using MediatR;
using Users.Domain.Entities;
using Users.Domain.Repositories;

namespace Users.Application.Features.Users.Queries.GetProfile;

public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, UserProfile?>
{
    private readonly IUserProfileRepository _profileRepository;

    public GetProfileQueryHandler(IUserProfileRepository profileRepository)
    {
        _profileRepository = profileRepository;
    }

    public async Task<UserProfile?> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        return await _profileRepository.GetByUserIdAndAppIdAsync(request.UserId, request.AppId);
    }
}
