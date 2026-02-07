using MediatR;
using Users.Application.Common.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Users.Domain.Repositories;

namespace Users.Application.Features.Users.Queries.GetDashboardStats;

public record GetDashboardStatsQuery : IRequest<DashboardStatsDto>;

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
        var profiles = await _profileRepository.ListAsync();

        var totalUsers = profiles.Select(p => p.UserId).Distinct().Count();
        var activeUsers = totalUsers; // Currently, we count all unique users as active. TODO: Filter by Login/Status
        
        // New Users Last 24h
        var oneDayAgo = DateTime.UtcNow.AddHours(-24);
        // Handle Created potentially being default/minvalue if not set correctly in older data
        var newUsers = profiles.Where(p => p.Created >= oneDayAgo).Select(p => p.UserId).Distinct().Count();

        // Users Per App
        var usersPerApp = profiles
            .GroupBy(p => p.AppId)
            .Select(g => new AppUserCountDto(g.Key.ToString(), g.Count()))
            .ToList();

        return new DashboardStatsDto(totalUsers, activeUsers, newUsers, usersPerApp);
    }
}
