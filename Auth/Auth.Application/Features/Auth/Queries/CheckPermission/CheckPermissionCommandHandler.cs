using MediatR;
using Auth.Domain.Repositories;
using Auth.Domain.Entities;
using System.Linq;

namespace Auth.Application.Features.Auth.Queries.CheckPermission;

public class CheckPermissionCommandHandler : IRequestHandler<CheckPermissionQuery, bool>
{
    private readonly IUserRepository _userRepository;

    public CheckPermissionCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<bool> Handle(CheckPermissionQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserWithRolesAsync(request.UserId);
        if (user == null) return false;

        // 1. Check if global admin (SuperAdmin role)
        // Hardcoded System App ID for now
        var systemAppId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        if (user.Memberships.Any(m => m.AppId == systemAppId && m.Role?.Name == "SuperAdmin")) return true;

        // 2. Find membership for the requested App
        var membership = user.Memberships.FirstOrDefault(m => m.AppId == request.AppId);
        if (membership == null) return false;

        // 3. Check Status
        if (user.Status != GlobalUserStatus.Active) return false;
        if (membership.Status != AppUserStatus.Active) return false;

        // 4. Check Permission in Role
        if (membership.Role == null) return false;

        return membership.Role.Permissions.Any(p => p.Name == request.PermissionName);
    }
}
