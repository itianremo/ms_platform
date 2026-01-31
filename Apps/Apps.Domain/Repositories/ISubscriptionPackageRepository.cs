using Apps.Domain.Entities;
using Shared.Kernel;

namespace Apps.Domain.Repositories;

public interface ISubscriptionPackageRepository : IRepository<SubscriptionPackage>
{
    Task<List<SubscriptionPackage>> GetByAppIdAsync(Guid appId);
}
