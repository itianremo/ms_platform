using MediatR;
using Shared.Kernel;
using Auth.Domain.Entities;
using Auth.Domain.Repositories;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Auth.Application.Features.Auth.Queries.GetAppUserStats;

public record GetAppUserStatsQuery : IRequest<List<AppUserStatsDto>>
{
    public Guid? FilterAppId { get; set; }
}

public class GetAppUserStatsQueryHandler : IRequestHandler<GetAppUserStatsQuery, List<AppUserStatsDto>>
{
    private readonly IRepository<Role> _roleRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GetAppUserStatsQueryHandler> _logger;

    public GetAppUserStatsQueryHandler(
        IRepository<Role> roleRepository, 
        IHttpClientFactory httpClientFactory,
        ILogger<GetAppUserStatsQueryHandler> logger)
    {
        _roleRepository = roleRepository;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public class AppDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class AppProfileDto
    {
        public Guid UserId { get; set; }
        public Guid AppId { get; set; }
        public Guid RoleId { get; set; }
    }

    public async Task<List<AppUserStatsDto>> Handle(GetAppUserStatsQuery request, CancellationToken cancellationToken)
    {
        // Memberships logic removed. Handled via profile RoleId.
        var roles = await _roleRepository.ListAsync(r => true);

        var validProfiles = new List<AppProfileDto>();
        var appNames = new Dictionary<Guid, string>();
        
        try
        {
            var client = _httpClientFactory.CreateClient();
            
            // 1. Fetch App names from Apps Microservice
            var appsResponse = await client.GetAsync("http://ms_apps:8080/api/Apps", cancellationToken);
            if (appsResponse.IsSuccessStatusCode)
            {
                var content = await appsResponse.Content.ReadAsStringAsync(cancellationToken);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var apps = JsonSerializer.Deserialize<List<AppDto>>(content, options);
                if (apps != null)
                {
                    appNames = apps.ToDictionary(a => a.Id, a => a.Name);
                }
            }
            else
            {
                _logger.LogWarning($"Failed to fetch apps from ms_apps. Status code: {appsResponse.StatusCode}");
            }

            // 2. Fetch active Profile IDs from Users Microservice
            var profilesResponse = await client.GetAsync("http://ms_users:8080/api/Users/profile-ids", cancellationToken);
            if (profilesResponse.IsSuccessStatusCode)
            {
                var content = await profilesResponse.Content.ReadAsStringAsync(cancellationToken);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var profiles = JsonSerializer.Deserialize<List<AppProfileDto>>(content, options);
                if (profiles != null)
                {
                    validProfiles = profiles;
                }
            }
            else
            {
                _logger.LogWarning($"Failed to fetch profiles from ms_users. Status code: {profilesResponse.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while fetching external data from ms_apps or ms_users");
        }

        var umpAppId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        // Apply AppId Filter
        if (request.FilterAppId.HasValue && request.FilterAppId.Value != umpAppId)
        {
            if (!appNames.ContainsKey(request.FilterAppId.Value))
            {
                throw new global::Auth.Domain.Exceptions.NotFoundException("App not found");
            }
            
            validProfiles = validProfiles.Where(p => p.AppId == request.FilterAppId.Value).ToList();
            
            // Filter our dictionary to just the requested AppId so the default inclusion logic only returns the one app
            appNames = new Dictionary<Guid, string> { { request.FilterAppId.Value, appNames[request.FilterAppId.Value] } };
        }

        var results = new List<AppUserStatsDto>();

        foreach (var app in appNames)
        {
            var appId = app.Key;
            var appName = app.Value;

            var appProfiles = validProfiles.Where(p => p.AppId == appId).ToList();
            int adminCount = 0;
            int visitorCount = 0;

            foreach (var profile in appProfiles)
            {
                // Find role for this user in this app
                var userRoles = roles.Where(r => r.Id == profile.RoleId).ToList();

                bool isAdmin = userRoles.Any(r => 
                    r.Name.Contains("Admin", StringComparison.OrdinalIgnoreCase) || 
                    r.Name.Contains("Manage", StringComparison.OrdinalIgnoreCase));

                if (isAdmin)
                {
                    adminCount++;
                }
                else
                {
                    visitorCount++;
                }
            }

            results.Add(new AppUserStatsDto
            {
                AppId = appId,
                AppName = appName,
                AdminCount = adminCount,
                VisitorCount = visitorCount
            });
        }

        return results;
    }
}
