using Search.Domain.Entities;
using Shared.Kernel;

namespace Search.Domain.Repositories;

public interface ISearchRepository : IRepository<UserSearchProfile>
{
    Task<List<UserSearchProfile>> SearchAsync(Guid appId, string query, int limit = 20);
}
