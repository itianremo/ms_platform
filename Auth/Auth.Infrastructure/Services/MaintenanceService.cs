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
        // App-level data reset is now managed by Users.API or event propagation
        await Task.CompletedTask;
    }
}
