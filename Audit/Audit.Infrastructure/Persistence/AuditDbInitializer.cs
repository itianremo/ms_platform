using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Audit.Infrastructure.Persistence;

public class AuditDbInitializer
{
    private readonly AuditDbContext _context;
    private readonly ILogger<AuditDbInitializer> _logger;

    public AuditDbInitializer(AuditDbContext context, ILogger<AuditDbInitializer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            if (_context.Database.IsSqlServer())
            {
                await _context.Database.MigrateAsync();
            }
        }
        catch (Exception ex)
        {
            // Ignore (logging as warning) if database already exists or other ensure-created issues, 
            // so the service can proceed to run.
            _logger.LogWarning(ex, "An error occurred while initializing the database. Continuing...");
        }
    }
}
