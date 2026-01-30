using Microsoft.EntityFrameworkCore;
using Notifications.Application.Common.Interfaces;
using Notifications.Domain.Entities;
using Notifications.Infrastructure.Persistence;

namespace Notifications.Infrastructure.Repositories;

public class NotificationConfigRepository : INotificationConfigRepository
{
    private readonly NotificationsDbContext _context;

    public NotificationConfigRepository(NotificationsDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationConfig?> GetByTypeAsync(string type, CancellationToken cancellationToken)
    {
        try
        {
            // Assuming we want the active one, or just the one for the type if we enforce single provider per type
            return await _context.NotificationConfigs
                .FirstOrDefaultAsync(x => x.Type == type, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DB Connection Failed: {ex.Message}. Falling back to JSON seed.");
            return await GetFromSeedAsync(type);
        }
    }

    private async Task<NotificationConfig?> GetFromSeedAsync(string type)
    {
        try 
        {
            var seedPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Persistence", "Seed", "notification_configs.json");
            if (!File.Exists(seedPath)) return null;

            var json = await File.ReadAllTextAsync(seedPath);
            var configs = System.Text.Json.JsonSerializer.Deserialize<List<NotificationConfig>>(json);
            return configs?.FirstOrDefault(x => x.Type == type);
        }
        catch
        {
            return null;
        }
    }

    public async Task AddAsync(NotificationConfig config, CancellationToken cancellationToken)
    {
        await _context.NotificationConfigs.AddAsync(config, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(NotificationConfig config, CancellationToken cancellationToken)
    {
        _context.NotificationConfigs.Update(config);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<NotificationConfig>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.NotificationConfigs.ToListAsync(cancellationToken);
    }
}
