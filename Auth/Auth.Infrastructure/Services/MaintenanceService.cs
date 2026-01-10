using Auth.Application.Common.Interfaces;
using Auth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Services;

public class MaintenanceService : IMaintenanceService
{
    private readonly AuthDbContext _context;

    public MaintenanceService(AuthDbContext context)
    {
        _context = context;
    }

    public async Task ResetAppDataAsync(Guid appId)
    {
        // Delete all memberships for this app
        await _context.UserAppMemberships
            .Where(m => m.AppId == appId)
            .ExecuteDeleteAsync();
    }
}
