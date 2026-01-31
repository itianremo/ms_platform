using MediatR;
using Users.Domain.Entities;
using Users.Domain.Repositories;

namespace Users.Application.Features.Users.Queries.GetProfile;

public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, UserProfile?>
{
    private readonly IUserProfileRepository _profileRepository;
    private readonly Shared.Kernel.Interfaces.ICacheService _cache;

    public GetProfileQueryHandler(IUserProfileRepository profileRepository, Shared.Kernel.Interfaces.ICacheService cache)
    {
        _profileRepository = profileRepository;
        _cache = cache;
    }

    public async Task<UserProfile?> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        string cacheKey = $"profile_{request.UserId}_{request.AppId}";
        var cached = await _cache.GetAsync<UserProfile>(cacheKey, cancellationToken);
        if (cached != null) return cached;

        var profile = await _profileRepository.GetByUserIdAndAppIdAsync(request.UserId, request.AppId);
        if (profile != null)
        {
            await _cache.SetAsync(cacheKey, profile, TimeSpan.FromMinutes(5), cancellationToken);
        }
        
        return profile;
    }
}
