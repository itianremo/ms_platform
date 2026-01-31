using MediatR;
using Apps.Domain.Repositories;

namespace Apps.Application.Features.Apps.Queries.GetPackagesByApp;

public class GetPackagesByAppQueryHandler : IRequestHandler<GetPackagesByAppQuery, List<SubscriptionPackageDto>>
{
    private readonly ISubscriptionPackageRepository _repository;

    public GetPackagesByAppQueryHandler(ISubscriptionPackageRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<SubscriptionPackageDto>> Handle(GetPackagesByAppQuery request, CancellationToken cancellationToken)
    {
        var packages = await _repository.GetByAppIdAsync(request.AppId);
        
        return packages.Select(p => new SubscriptionPackageDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            Period = (int)p.Period,
            Currency = p.Currency
        }).ToList();
    }
}
