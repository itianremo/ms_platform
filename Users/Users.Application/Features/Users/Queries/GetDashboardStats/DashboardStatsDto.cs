using System.Collections.Generic;

namespace Users.Application.Features.Users.Queries.GetDashboardStats;

public record AppUserCountDto(string AppName, int Count);

public record DashboardStatsDto(
    int TotalUsers, 
    int ActiveUsers, 
    int NewUsersLast24h,
    List<AppUserCountDto> UsersPerApp
);
