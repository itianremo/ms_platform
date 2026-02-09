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
            if (existingApp != null && seed.Id != Guid.Empty && existingApp.Id != seed.Id) 
            {
                logger.LogWarning("App {Name} exists with wrong ID {ExistingId}. Expecting {ExpectedId}. Deleting to re-seed.", seed.Name, existingApp.Id, seed.Id);
                context.Apps.Remove(existingApp);
                await context.SaveChangesAsync();
                existingApp = null;
            }

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
        // Seed Packages
        var seedPackagesPath = Path.Combine(AppContext.BaseDirectory, "seed-packages.json");
        if (File.Exists(seedPackagesPath))
        {
            var jsonPackages = await File.ReadAllTextAsync(seedPackagesPath);
            var seedPackages = JsonSerializer.Deserialize<List<SeedPackageDto>>(jsonPackages);
            
            if (seedPackages != null)
            {
                foreach (var sp in seedPackages)
                {
                    // Check if package exists for this App by Name (Unique Name per App assumption)
                    var existingPkg = await context.SubscriptionPackages
                        .FirstOrDefaultAsync(p => p.AppId == sp.AppId && p.Name == sp.Name);
                    
                    if (existingPkg == null)
                    {
                        var pkg = new SubscriptionPackage(sp.AppId, sp.Name, sp.Description, sp.Price, sp.Discount, (SubscriptionPeriod)sp.Period);
                        await context.SubscriptionPackages.AddAsync(pkg);
                        logger.LogInformation("Seeded Package: {Name} for App {AppId}", sp.Name, sp.AppId);
                    }
                }
                await context.SaveChangesAsync();
            }
        }

        // Hardcoded Seeding for Wissler (Dynamic Request)
        var wisslerAppId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var wisslerPackages = new List<(string Name, decimal Price, SubscriptionPeriod Period)>
        {
            ("1 Week", 49, SubscriptionPeriod.Weekly),
            ("2 Weeks", 89, SubscriptionPeriod.BiWeekly),
            ("1 Month", 149, SubscriptionPeriod.Monthly),
            ("3 Months", 399, SubscriptionPeriod.Quarterly),
            ("6 Months", 649, SubscriptionPeriod.SemiAnnually),
            ("1 Year", 1000, SubscriptionPeriod.Yearly),
            ("Unlimited", 1700, SubscriptionPeriod.Unlimited)
        };

        foreach (var (name, price, period) in wisslerPackages)
        {
             var existingPkg = await context.SubscriptionPackages
                .FirstOrDefaultAsync(p => p.AppId == wisslerAppId && p.Name == name);

            if (existingPkg == null)
            {
                var pkg = new SubscriptionPackage(
                    wisslerAppId, 
                    name, 
                    $"Access to premium features for {name}", 
                    price, 
                    0, 
                    period,
                    "EGP"
                );
                await context.SubscriptionPackages.AddAsync(pkg);
                logger.LogInformation("Seeded Wissler Package: {Name} - {Price} EGP", name, price);
            }
        }
        await context.SaveChangesAsync();

        // Ensure VIP Unlimited Package exists for ALL Apps (for Admin Auto-Grant)
        var allApps = await context.Apps.ToListAsync();
        foreach (var app in allApps)
        {
            // Seed Privacy Policy for Wissler
            if (app.Id == Guid.Parse("22222222-2222-2222-2222-222222222222"))
            {
                var policy = @"# Privacy Policy for Wissler

**Effective Date:** October 2023

## 1. Introduction
Welcome to Wissler. We respect your privacy and are committed to protecting your personal data. This privacy policy will inform you as to how we look after your personal data when you visit our application and tell you about your privacy rights and how the law protects you, particularly in compliance with the Egyptian Personal Data Protection Law No. 151 of 2020.

## 2. Important Information and Who We Are
Wissler is the controller and responsible for your personal data.

## 3. The Data We Collect About You
We may collect, use, store and transfer different kinds of personal data about you which we have grouped together follows:
- **Identity Data:** includes first name, last name, username or similar identifier, marital status, title, date of birth and gender.
- **Contact Data:** includes email address and telephone numbers.
- **Location Data:** includes your current location for matching purposes.

## 4. How We Use Your Personal Data
We will only use your personal data when the law allows us to. Most commonly, we will use your personal data in the following circumstances:
- Where we need to perform the contract we are about to enter into or have entered into with you.
- Where it is necessary for our legitimate interests (or those of a third party) and your interests and fundamental rights do not override those interests.
- Where we need to comply with a legal or regulatory obligation.

## 5. Data Security
We have put in place appropriate security measures to prevent your personal data from being accidentally lost, used or accessed in an unauthorized way, altered or disclosed.

## 6. Your Legal Rights
Under certain circumstances, you have rights under data protection laws in relation to your personal data, including the right to access, correct, erase, restrict, transfer, or object to processing of your personal data.";
                
                if (string.IsNullOrEmpty(app.PrivacyPolicy) || app.PrivacyPolicy.Length < 10)
                {
                    app.UpdatePrivacyPolicy(policy);
                    logger.LogInformation("Seeded Privacy Policy for Wissler App");
                }
            }

            var vipPackage = await context.SubscriptionPackages
                .FirstOrDefaultAsync(p => p.AppId == app.Id && p.Name == "VIP Unlimited");
            
            if (vipPackage == null)
            {
                // Create VIP Unlimited
                var newVip = new SubscriptionPackage(app.Id, "VIP Unlimited", "Auto-granted to Admins", 0, 0, SubscriptionPeriod.Unlimited);
                await context.SubscriptionPackages.AddAsync(newVip);
                logger.LogInformation("Seeded VIP Unlimited Package for App {Name}", app.Name);
            }
        }
        await context.SaveChangesAsync();
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

    private class SeedPackageDto
    {
        public Guid AppId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public int Period { get; set; }
    }
}
