using MediatR;
using Apps.Domain.Entities;
using Apps.Domain.Repositories;

namespace Apps.Application.Features.Apps.Queries.GetAllApps;

public class GetAllAppsQueryHandler : IRequestHandler<GetAllAppsQuery, List<AppConfig>>
{
    private readonly IAppRepository _repository;

    public GetAllAppsQueryHandler(IAppRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<AppConfig>> Handle(GetAllAppsQuery request, CancellationToken cancellationToken)
    {
        // In a real scenario, this should be paginated
        return await _repository.ListAsync();
    }
}
