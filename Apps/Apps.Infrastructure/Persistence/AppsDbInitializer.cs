using System.Text.Json;
using Microsoft.Extensions.Logging;
using Apps.Domain.Entities;
using Shared.Kernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Apps.Infrastructure.Persistence;

public static class AppsDbInitializer
{
    public static async Task SeedAsync(AppsDbContext context, ILogger logger)
    {
            // Robust Retry Logic for Container Cold Starts
            int maxRetries = 10;
            int delaySeconds = 2;
            
            for (int i = 0; i < maxRetries; i++)
            {
                try 
                {
                    // Ensure DB Created and Migrated
                    await context.Database.MigrateAsync();
                    
                    // No need for explicit Table check with Migrations, but good for sanity
                    // var databaseCreator = context.Database.GetService<Microsoft.EntityFrameworkCore.Storage.IRelationalDatabaseCreator>();
                    // if (!await databaseCreator.HasTablesAsync()) ...
                    
                    logger.LogInformation("Database initialization check passed.");
                    break; // Success
                }
                catch (Exception ex)
                {
                    if (i == maxRetries - 1)
                    {
                        logger.LogError(ex, "Critical: Database initialization failed after {Retries} attempts. Giving up.", maxRetries);
                        throw; // Rethrow to stop startup since this is critical
                    }
                    
                    logger.LogWarning(ex, "Database connection failed (Attempt {Attempt}/{Max}). Retrying in {Delay}s...", i + 1, maxRetries, delaySeconds);
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                     // Exponential Backoff? Or Linear? Linear is fine for startup.
                }
            }


        var seedDataPath = Path.Combine(AppContext.BaseDirectory, "seed-apps.json");
        if (!File.Exists(seedDataPath))
        {
            logger.LogWarning("Seed apps file not found at {Path}", seedDataPath);
            return;
        }

        var json = await File.ReadAllTextAsync(seedDataPath);
        var seedApps = JsonSerializer.Deserialize<List<SeedAppDto>>(json); // Default is case-sensitive or use options

        if (seedApps == null) return;

        foreach (var seed in seedApps)
        {
            // Check if app exists by ID or Name to avoid duplicates
            var existingApp = await context.Apps.FirstOrDefaultAsync(a => a.Id == seed.Id || a.Name == seed.Name);
            if (existingApp == null)
            {
                // Create New
                var newApp = new AppConfig(seed.Name, seed.Description, seed.BaseUrl, seed.Id);
                newApp.UpdateTheme(seed.ThemeJson);
                newApp.UpdateDefaultUserProfile(seed.DefaultUserProfileJson ?? "{}");
                newApp.UpdateExternalAuthProviders(seed.ExternalAuthProvidersJson ?? "{}");
                newApp.UpdateVerificationType((VerificationType)seed.VerificationType);
                newApp.UpdateRequirements(seed.RequiresAdminApproval);
                
                if (seed.IsActive) newApp.Activate(); else newApp.Deactivate();

                await context.Apps.AddAsync(newApp);
                logger.LogInformation("Seeded App: {Name}", seed.Name);
            }
            else
            {
                // Update specific fields if needed
                if (string.IsNullOrEmpty(existingApp.DefaultUserProfileJson) || existingApp.DefaultUserProfileJson == "{}")
                {
                    if (!string.IsNullOrEmpty(seed.DefaultUserProfileJson) && seed.DefaultUserProfileJson != "{}")
                    {
                        existingApp.UpdateDefaultUserProfile(seed.DefaultUserProfileJson);
                        logger.LogInformation("Updated App {Name} with DefaultUserProfileJson", seed.Name);
                    }
                }
                
                // Update ExternalAuthProvidersJson if missing
                if (string.IsNullOrEmpty(existingApp.ExternalAuthProvidersJson) || existingApp.ExternalAuthProvidersJson == "{}")
                {
                    if (!string.IsNullOrEmpty(seed.ExternalAuthProvidersJson) && seed.ExternalAuthProvidersJson != "{}")
                    {
                        existingApp.UpdateExternalAuthProviders(seed.ExternalAuthProvidersJson);
                        logger.LogInformation("Updated App {Name} with ExternalAuthProvidersJson", seed.Name);
                    }
                }

                // Targeted Fix for System App (Admin Dashboard)
                // If it exists but has VerificationType = None (0) and we expect it to be secure, fix it.
                // This handles the dev environment case where the app was seeded before security was enforced.
                if (existingApp.Id == Guid.Parse("00000000-0000-0000-0000-000000000001") && 
                    existingApp.VerificationType == VerificationType.None)
                {
                    existingApp.UpdateVerificationType((VerificationType)seed.VerificationType);
                    existingApp.UpdateRequirements(seed.RequiresAdminApproval);
                    // Force activation if inactive?
                    if (!existingApp.IsActive && seed.IsActive) existingApp.Activate();
                    
                    logger.LogInformation("System App {Name} found with insecure settings. Updated to match seed.", seed.Name);
                }
                else 
                {
                    logger.LogInformation("App {Name} already exists, skipping seed.", seed.Name);
                }
            }
        }
        
        try
        {
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Log but don't crash if there's a duplicate key error
            logger.LogWarning(ex, "Error saving apps seed data (likely duplicates). Continuing...");
        }
    }

    private class SeedAppDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string BaseUrl { get; set; }
        public string ThemeJson { get; set; }
        public string DefaultUserProfileJson { get; set; }
        public string ExternalAuthProvidersJson { get; set; } // Added
        public bool IsActive { get; set; }
        public int VerificationType { get; set; }
        public bool RequiresAdminApproval { get; set; }
    }
}
