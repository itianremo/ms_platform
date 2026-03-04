using MediatR;
using Users.Domain.Entities;
using Users.Domain.Repositories;
using Users.Application.DTOs;
using System.Text.Json;

namespace Users.Application.Features.Users.Queries.GetProfile;

public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, UserProfileDto?>
{
    private readonly IUserProfileRepository _profileRepository;
    private readonly Shared.Kernel.Interfaces.ICacheService _cache;

    public GetProfileQueryHandler(IUserProfileRepository profileRepository, Shared.Kernel.Interfaces.ICacheService cache)
    {
        _profileRepository = profileRepository;
        _cache = cache;
    }

    public async Task<UserProfileDto?> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        string cacheKey = $"profile_{request.UserId}_{request.AppId}";
        var cached = await _cache.GetAsync<UserProfileDto>(cacheKey, cancellationToken);
        if (cached != null) return cached;

        var profile = await _profileRepository.GetByUserIdAndAppIdAsync(request.UserId, request.AppId);
        if (profile != null)
        {
            var dto = new UserProfileDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                AppId = profile.AppId,
                DisplayName = profile.DisplayName,
                AvatarUrl = profile.AvatarUrl,
                Bio = profile.Bio,
                RoleId = profile.RoleId,
                LoyaltyPoints = profile.LoyaltyPoints,
                CoinsBalance = profile.CoinsBalance,
                DateOfBirth = profile.DateOfBirth,
                Gender = profile.Gender,
                Created = profile.Created,
                LastActiveAt = profile.LastActiveAt
            };

            if (!string.IsNullOrEmpty(profile.CustomDataJson) && profile.CustomDataJson != "{}")
            {
                try
                {
                    dto.DynamicData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(profile.CustomDataJson);
                }
                catch { /* Ignore invalid JSON */ }
            }

            await _cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(5), cancellationToken);
            return dto;
        }
        
        return null;
    }
}
