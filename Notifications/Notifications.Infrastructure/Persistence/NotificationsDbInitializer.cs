using Microsoft.EntityFrameworkCore;
using Notifications.Domain.Entities;
using System.Text.Json;

namespace Notifications.Infrastructure.Persistence;

public class NotificationsDbInitializer
{
    private readonly NotificationsDbContext _context;

    public NotificationsDbInitializer(NotificationsDbContext context)
    {
        _context = context;
    }

    public async Task InitializeAsync()
    {
        try
        {
             // Use EnsureCreated to creating tables if missing (works if DB doesn't exist)
             // If DB exists, it might throw "Already Exists" if checking failed, 
             // but we catch it.
             // Ideally we want to ensure schema. 
             await _context.Database.EnsureCreatedAsync();
        }
        catch (Exception ex)
        {
             // Log warning and continue
             Console.WriteLine($"EnsureCreated failed: {ex.Message}");
        }


        if (!await _context.NotificationConfigs.AnyAsync())
        {
            var seedPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Persistence", "Seed", "notification_configs.json");
            if (!File.Exists(seedPath)) return;

            var json = await File.ReadAllTextAsync(seedPath);
            var configs = JsonSerializer.Deserialize<List<NotificationConfig>>(json);

            if (configs != null)
            {
                await _context.NotificationConfigs.AddRangeAsync(configs);
                await _context.SaveChangesAsync();
            }
        }
    }
}
