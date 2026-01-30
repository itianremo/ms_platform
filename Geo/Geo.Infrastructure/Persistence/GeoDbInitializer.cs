using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Geo.Infrastructure.Persistence;

public class GeoDbInitializer
{
    private readonly GeoDbContext _context;
    private readonly ILogger<GeoDbInitializer> _logger;

    public GeoDbInitializer(GeoDbContext context, ILogger<GeoDbInitializer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            if (_context.Database.IsNpgsql())
            {
                await _context.Database.MigrateAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }
}
