using Shared.Kernel;
using Users.Domain.Entities;

namespace Users.Domain.Repositories;

public interface IUserProfileRepository : IRepository<UserProfile>
{
    Task<UserProfile?> GetByUserIdAndAppIdAsync(Guid userId, Guid appId);
}
