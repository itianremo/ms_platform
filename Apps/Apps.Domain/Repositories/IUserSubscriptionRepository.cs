using Apps.Domain.Entities;

namespace Apps.Domain.Repositories;

public interface IUserSubscriptionRepository
{
    Task<UserSubscription?> GetByIdAsync(Guid id);
    Task<List<UserSubscription>> GetByUserIdAsync(Guid appId, Guid userId);
    Task AddAsync(UserSubscription subscription);
    Task UpdateAsync(UserSubscription subscription);
}
