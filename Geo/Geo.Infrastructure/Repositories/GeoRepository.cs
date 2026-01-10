using Geo.Domain.Entities;
using Geo.Domain.Repositories;
using Geo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using Shared.Kernel;
using System.Linq.Expressions;

namespace Geo.Infrastructure.Repositories;

public class GeoRepository : IGeoRepository
{
    private readonly GeoDbContext _context;
    private readonly GeometryFactory _geometryFactory;

    public GeoRepository(GeoDbContext context)
    {
        _context = context;
        _geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
    }

    public async Task<GeoLocation> AddAsync(GeoLocation entity)
    {
        await _context.Locations.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(GeoLocation entity)
    {
        _context.Locations.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<GeoLocation?> GetByIdAsync(Guid id)
    {
        return await _context.Locations.FindAsync(id);
    }

    public async Task<GeoLocation?> GetByUserIdAsync(int userId, int appId)
    {
        return await _context.Locations.FirstOrDefaultAsync(x => x.UserId == userId && x.AppId == appId);
    }

    public async Task<List<GeoLocation>> GetNearbyAsync(int appId, double lat, double lon, double radiusKm)
    {
        var currentLocation = _geometryFactory.CreatePoint(new Coordinate(lon, lat));
        
        // PostGIS "ST_DWithin" equivalent in EF Core Npgsql.
        // Npgsql maps IsWithinDistance to ST_DWithin if using geography type
        // Note: radius in IsWithinDistance for geography type is in meters
        
        double radiusMeters = radiusKm * 1000;

        return await _context.Locations
            .Where(x => x.AppId == appId && x.Location.IsWithinDistance(currentLocation, radiusMeters))
            .OrderBy(x => x.Location.Distance(currentLocation))
            .ToListAsync();
    }

    public async Task<List<GeoLocation>> ListAsync()
    {
        return await _context.Locations.ToListAsync();
    }

    public async Task<List<GeoLocation>> ListAsync(Expression<Func<GeoLocation, bool>> predicate)
    {
        return await _context.Locations.Where(predicate).ToListAsync();
    }

    public async Task UpdateAsync(GeoLocation entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }
}
