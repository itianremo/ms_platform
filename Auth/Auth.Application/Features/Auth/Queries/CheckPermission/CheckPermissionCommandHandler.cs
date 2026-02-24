using MediatR;
using Auth.Domain.Repositories;
using Auth.Domain.Entities;
using System.Linq;
using System.Net.Http.Json;

namespace Auth.Application.Features.Auth.Queries.CheckPermission;

public class CheckPermissionCommandHandler : IRequestHandler<CheckPermissionQuery, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly Shared.Kernel.Interfaces.ICacheService _cache;
    private readonly IHttpClientFactory _httpClientFactory;

    public CheckPermissionCommandHandler(IUserRepository userRepository, Shared.Kernel.Interfaces.ICacheService cache, IHttpClientFactory httpClientFactory)
    {
        _userRepository = userRepository;
        _cache = cache;
        _httpClientFactory = httpClientFactory;
    }

    public class LocalAuthProfileDto
    {
        public Guid RoleId { get; set; }
        public int Status { get; set; }
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
        var systemAppId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        try 
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("http://ms_users:8080/"); // Standard internal DNS

            // Helper to check permissions
            async Task<bool> CheckAppPermissions(Guid targetAppId)
            {
                var response = await client.GetAsync($"api/Internal/auth-profile?userId={request.UserId}&appId={targetAppId}", cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    var profile = await response.Content.ReadFromJsonAsync<LocalAuthProfileDto>(cancellationToken: cancellationToken);
                    if (profile != null && user.Status == GlobalUserStatus.Active && profile.Status == 0) // AppUserStatus.Active
                    {
                        var role = await _userRepository.GetRoleByIdAsync(profile.RoleId);
                        if (role != null)
                        {
                            if (role.Name == "SuperAdmin") return true;
                            if (role.Permissions != null && role.Permissions.Any(p => p.Name == request.PermissionName)) return true;
                        }
                    }
                }
                return false;
            }

            // 1. Check Global SuperAdmin
            result = await CheckAppPermissions(systemAppId);

            // 2. Check Specific App Permissions
            if (!result && request.AppId != systemAppId)
            {
                result = await CheckAppPermissions(request.AppId);
            }
        }
        catch 
        {
            // Fallback or log if Users.API is down
            result = false;
        }
        
        await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5), cancellationToken);
        return result;
    }
}
