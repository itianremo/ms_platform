using MediatR;
using Apps.Domain.Entities;
using Apps.Domain.Repositories;

namespace Apps.Application.Features.Apps.Queries.GetAllApps;

public class GetAllAppsQueryHandler : IRequestHandler<GetAllAppsQuery, List<AppDto>>
{
    private readonly IAppRepository _repository;
    private readonly Shared.Kernel.Interfaces.ICacheService _cache;

    public GetAllAppsQueryHandler(IAppRepository repository, Shared.Kernel.Interfaces.ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<List<AppDto>> Handle(GetAllAppsQuery request, CancellationToken cancellationToken)
    {
        string cacheKey = "apps_all";
        var cached = await _cache.GetAsync<List<AppDto>>(cacheKey, cancellationToken);
        if (cached != null) return cached;

        // In a real scenario, this should be paginated
        var appsEntities = await _repository.ListAsync();
        var apps = appsEntities.Select(a => a.ToDto()).ToList();
        
        await _cache.SetAsync(cacheKey, apps, TimeSpan.FromMinutes(30), cancellationToken);
        
        return apps;
    }
}
