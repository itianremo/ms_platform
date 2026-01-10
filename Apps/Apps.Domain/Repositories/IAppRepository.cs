using Apps.Domain.Entities;
using Shared.Kernel;

namespace Apps.Domain.Repositories;

public interface IAppRepository : IRepository<AppConfig>
{
    Task<AppConfig?> GetByNameAsync(string name);
}
