using MediatR;
using Users.Application.Common.Interfaces;

namespace Users.Application.Features.Users.Queries.GetDashboardStats;



public record DashboardStatsDto(
    int TotalUsers, 
    int ActiveUsers, 
    int NewUsersLast24h,
    List<AppUserCountDto> UsersPerApp
);

public record AppUserCountDto(string AppName, int Count);

public record GetDashboardStatsQuery : IRequest<DashboardStatsDto>;

public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    public GetDashboardStatsQueryHandler()
    {
    }

    public Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        // Stubbed to fix build: Repo reference issue in Users.Application
        return Task.FromResult(new DashboardStatsDto(0, 0, 0, new List<AppUserCountDto>()));
    }
}
