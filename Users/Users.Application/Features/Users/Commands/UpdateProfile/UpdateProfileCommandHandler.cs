using MediatR;
using Users.Domain.Entities;
using Users.Domain.Repositories;

namespace Users.Application.Features.Users.Commands.UpdateProfile;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand>
{
    private readonly IUserProfileRepository _profileRepository;
    private readonly Shared.Kernel.Interfaces.ICurrentUserService _currentUserService;

    public UpdateProfileCommandHandler(IUserProfileRepository profileRepository, Shared.Kernel.Interfaces.ICurrentUserService currentUserService)
    {
        _profileRepository = profileRepository;
        _currentUserService = currentUserService;
    }

    public async Task Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await _profileRepository.GetByUserIdAndAppIdAsync(request.UserId, request.AppId);
        
        if (profile == null)
        {
            // Create if not exists (Lazy creation)
            // Create if not exists (Lazy creation)
            profile = new UserProfile(request.UserId, request.AppId, request.DisplayName, _currentUserService.Email ?? string.Empty);
            await _profileRepository.AddAsync(profile);
        }

        profile.UpdateProfile(request.DisplayName, request.Bio, request.AvatarUrl, request.CustomDataJson, request.DateOfBirth, request.Gender);
        
        await _profileRepository.UpdateAsync(profile);
    }
}
