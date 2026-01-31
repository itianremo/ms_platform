using MediatR;
using Apps.Domain.Repositories;

namespace Apps.Application.Features.Subscriptions.Queries.GetUserSubscriptions;

public record GetUserSubscriptionsQuery(Guid AppId, Guid UserId) : IRequest<List<UserSubscriptionDto>>;

public class GetUserSubscriptionsQueryHandler : IRequestHandler<GetUserSubscriptionsQuery, List<UserSubscriptionDto>>
{
    private readonly IUserSubscriptionRepository _subscriptionRepository;
    private readonly ISubscriptionPackageRepository _packageRepository;

    public GetUserSubscriptionsQueryHandler(
        IUserSubscriptionRepository subscriptionRepository,
        ISubscriptionPackageRepository packageRepository)
    {
        _subscriptionRepository = subscriptionRepository;
        _packageRepository = packageRepository;
    }

    public async Task<List<UserSubscriptionDto>> Handle(GetUserSubscriptionsQuery request, CancellationToken cancellationToken)
    {
        var subs = await _subscriptionRepository.GetByUserIdAsync(request.AppId, request.UserId);

        var result = new List<UserSubscriptionDto>();

        // Inefficient N+1 if repository verifies one by one? 
        // Or fetch all packages? 
        // Repository should support bulk fetch or I iterate.
        // Assuming small number of packages, iterating is okay or fetch all packages for app?
        // _packageRepository.GetByAppIdAsync(request.AppId) -> fetch all packages once.
        var appPackages = await _packageRepository.GetByAppIdAsync(request.AppId);
        var pkgMap = appPackages.ToDictionary(p => p.Id, p => p.Name);

        foreach (var s in subs)
        {
            result.Add(new UserSubscriptionDto
            {
                Id = s.Id,
                AppId = s.AppId,
                PackageId = s.PackageId,
                PackageName = pkgMap.ContainsKey(s.PackageId) ? pkgMap[s.PackageId] : "Unknown",
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                IsActive = s.IsActive,
                PricePaid = s.PricePaid
            });
        }
        return result;
    }
}
