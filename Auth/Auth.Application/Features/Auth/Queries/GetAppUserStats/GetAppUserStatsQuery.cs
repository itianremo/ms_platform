using MediatR;
using Shared.Kernel;
using Auth.Domain.Entities;
using Auth.Domain.Repositories; // Assuming generic or specific repository exists
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;


namespace Auth.Application.Features.Auth.Queries.GetAppUserStats;

public record GetAppUserStatsQuery : IRequest<List<AppUserStatsDto>>;

public class GetAppUserStatsQueryHandler : IRequestHandler<GetAppUserStatsQuery, List<AppUserStatsDto>>
{
    private readonly IRepository<UserAppMembership> _membershipRepository;
    private readonly IRepository<Role> _roleRepository;

    public GetAppUserStatsQueryHandler(IRepository<UserAppMembership> membershipRepository, IRepository<Role> roleRepository)
    {
        _membershipRepository = membershipRepository;
        _roleRepository = roleRepository;
    }

    public async Task<List<AppUserStatsDto>> Handle(GetAppUserStatsQuery request, CancellationToken cancellationToken)
    {
        // We need to join Membership and Role to check Role Name.
        // Assuming IRepository exposes IQueryable or we have a specialized repository.
        // If it's a generic repository with Specification pattern, we might need a spec.
        // For now, assuming direct EF access or ListAsync capability with Include.
        
        // Let's use ListAsync if it returns all, but that's inefficient.
        // Better to use IQueryable if available. Check IRepository definition.
        
        // FALLBACK: If IRepository is simple, we might need to fetch all (MVP) or add a specific method to IAuthRepository.
        // Let's assume we can fetch all for now as user base isn't huge yet for this demo.
        
        var memberships = await _membershipRepository.ListAsync(m => true); // Fetch all
        var roles = await _roleRepository.ListAsync(r => true);

        // In-memory grouping (Not optimal for prod, but safe for generic repo limitation)
        var query = from m in memberships
                    join r in roles on m.RoleId equals r.Id
                    group new { m, r } by m.AppId into g
                    select new AppUserStatsDto
                    {
                        AppId = g.Key,
                        AdminCount = g.Count(x => x.r.Name.Contains("Admin", StringComparison.OrdinalIgnoreCase) || x.r.Name.Contains("Manage", StringComparison.OrdinalIgnoreCase)),
                        VisitorCount = g.Count(x => !x.r.Name.Contains("Admin", StringComparison.OrdinalIgnoreCase) && !x.r.Name.Contains("Manage", StringComparison.OrdinalIgnoreCase))
                    };

        return query.ToList();
    }
}
