using MediatR;
using Auth.Domain.Repositories;
using Auth.Domain.Entities;
using System.Linq;

namespace Auth.Application.Features.Auth.Queries.CheckPermission;

public class CheckPermissionCommandHandler : IRequestHandler<CheckPermissionQuery, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly Shared.Kernel.Interfaces.ICacheService _cache;

    public CheckPermissionCommandHandler(IUserRepository userRepository, Shared.Kernel.Interfaces.ICacheService cache)
    {
        _userRepository = userRepository;
        _cache = cache;
    }

    public async Task<bool> Handle(CheckPermissionQuery request, CancellationToken cancellationToken)
    {
        string cacheKey = $"perm_{request.UserId}_{request.AppId}_{request.PermissionName}";
        var cached = await _cache.GetAsync<bool?>(cacheKey, cancellationToken);
        if (cached.HasValue) return cached.Value;

        var user = await _userRepository.GetUserWithRolesAsync(request.UserId);
        if (user == null) 
        {
             await _cache.SetAsync(cacheKey, false, TimeSpan.FromMinutes(5), cancellationToken);
             return false;
        }

        bool result = false;

        // 1. Check if global admin (SuperAdmin role)
        // Hardcoded System App ID for now
        var systemAppId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        // Check Global Permissions first
        if (request.AppId == Guid.Parse("00000000-0000-0000-0000-000000000001"))
        {
             // Optimization: If asking for permissions ON the global app, just check membership
        }
        
        if (user.Memberships.Any(m => m.AppId == systemAppId && m.Role?.Name == "SuperAdmin")) 
        {
            result = true;
        }
        else
        {
            // 2. Find membership for the requested App
            var membership = user.Memberships.FirstOrDefault(m => m.AppId == request.AppId);
            if (membership != null)
            {
                // 3. Check Status
                if (user.Status == GlobalUserStatus.Active && membership.Status == AppUserStatus.Active && membership.Role != null)
                {
                    // 4. Check Permission in Role
                    if (membership.Role.Permissions.Any(p => p.Name == request.PermissionName))
                    {
                        result = true;
                    }
                }
            }
        }
        
        await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5), cancellationToken);
        return result;
    }
}
