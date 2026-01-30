using MediatR;
using Users.Domain.Entities;
using Users.Domain.Repositories;

namespace Users.Application.Features.Users.Commands.UpdateProfile;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand>
{
    private readonly IUserProfileRepository _profileRepository;

    public UpdateProfileCommandHandler(IUserProfileRepository profileRepository)
    {
        _profileRepository = profileRepository;
    }

    public async Task Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await _profileRepository.GetByUserIdAndAppIdAsync(request.UserId, request.AppId);
        
        if (profile == null)
        {
            // Create if not exists (Lazy creation)
            profile = new UserProfile(request.UserId, request.AppId, request.DisplayName);
            await _profileRepository.AddAsync(profile);
        }

        profile.UpdateProfile(request.DisplayName, request.Bio, request.AvatarUrl, request.CustomDataJson, request.DateOfBirth, request.Gender);
        
        await _profileRepository.UpdateAsync(profile);
    }
}
