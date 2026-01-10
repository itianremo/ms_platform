using Microsoft.EntityFrameworkCore;
using Users.Application.Common.Interfaces;
using Users.Infrastructure.Persistence;

namespace Users.Infrastructure.Services;

public class MaintenanceService : IMaintenanceService
{
    private readonly UsersDbContext _context;

    public MaintenanceService(UsersDbContext context)
    {
        _context = context;
    }

    public async Task ResetAppDataAsync(Guid appId)
    {
        await _context.UserProfiles
            .Where(p => p.AppId == appId)
            .ExecuteDeleteAsync();
    }
}
