using Geo.Domain.Entities;
using Shared.Kernel;

namespace Geo.Domain.Repositories;

public interface IGeoRepository : IRepository<GeoLocation>
{
    Task<List<GeoLocation>> GetNearbyAsync(int appId, double lat, double lon, double radiusKm);
    Task<GeoLocation?> GetByUserIdAsync(int userId, int appId);
}
