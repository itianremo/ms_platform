using MediatR;
using Users.Application.Common.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Users.Domain.Repositories;

namespace Users.Application.Features.Users.Queries.GetDashboardStats;

public record GetDashboardStatsQuery(Guid? AppId = null, DateTime? StartDate = null, DateTime? EndDate = null) : IRequest<DashboardStatsDto>;

public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    private readonly IUserProfileRepository _profileRepository;

    public GetDashboardStatsQueryHandler(IUserProfileRepository profileRepository)
    {
        _profileRepository = profileRepository;
    }

    public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        // Fetch all profiles (Not efficient for large scale, but fine for MVP)
        var profilesList = await _profileRepository.ListAsync();
        IEnumerable<Domain.Entities.UserProfile> profiles = profilesList;
        
        if (request.AppId.HasValue)
        {
            profiles = profiles.Where(p => p.AppId == request.AppId.Value);
        }

        if (request.StartDate.HasValue)
        {
            profiles = profiles.Where(p => p.Created >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            profiles = profiles.Where(p => p.Created <= request.EndDate.Value);
        }

        var totalUsers = profiles.Select(p => p.UserId).Distinct().Count();
        
        var today = DateTime.UtcNow.Date;
        var activeUsers = profiles.Where(p => p.LastActiveAt >= today).Select(p => p.UserId).Distinct().Count();
        
        // New Users Last 24h
        var oneDayAgo = DateTime.UtcNow.AddHours(-24);
        // Handle Created potentially being default/minvalue if not set correctly in older data
        var newUsers = profiles.Where(p => p.Created >= oneDayAgo).Select(p => p.UserId).Distinct().Count();

        // Users Per App
        var usersPerApp = profiles
            .GroupBy(p => p.AppId)
            .Select(g => new AppUserCountDto(g.Key.ToString(), g.Count()))
            .ToList();

        // Calculate Demographics from CustomDataJson
        var demographics = new Dictionary<string, int>();
        foreach (var p in profiles)
        {
            if (!string.IsNullOrWhiteSpace(p.CustomDataJson) && p.CustomDataJson != "{}")
            {
                try
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(p.CustomDataJson);
                    if (doc.RootElement.TryGetProperty("countryId", out var countryProp))
                    {
                        var country = countryProp.GetString();
                        if (!string.IsNullOrWhiteSpace(country))
                        {
                            if (demographics.ContainsKey(country)) demographics[country]++;
                            else demographics[country] = 1;
                        }
                    }
                }
                catch { } // Ignore parse errors for individual records
            }
        }

        var demogList = demographics.Select(kv => new DemographicCountDto(kv.Key, kv.Value)).OrderByDescending(x => x.Count).ToList();
        
        // TODO: Query real matches when Match repository is integrated
        int totalMatches = 0;

        return new DashboardStatsDto(totalUsers, activeUsers, newUsers, usersPerApp, totalMatches, demogList);
    }
}
