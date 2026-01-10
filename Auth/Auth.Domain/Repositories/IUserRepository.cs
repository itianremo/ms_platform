using Auth.Domain.Entities;
using Shared.Kernel;

namespace Auth.Domain.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByLoginAsync(string loginProvider, string providerKey);
    Task<User?> GetUserWithRolesAsync(Guid userId);
    Task<List<User>> ListWithRolesAsync();
}
