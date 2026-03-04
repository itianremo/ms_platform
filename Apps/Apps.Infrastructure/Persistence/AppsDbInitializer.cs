using System.Text.Json;
using System.Text.Json.Serialization;
using Apps.Domain.Entities;
using Apps.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Kernel;

namespace Apps.Infrastructure.Persistence;

public class AppsDbInitializer
{
    public static async Task InitializeAsync(AppsDbContext context, ILogger logger)
    {
        try
        {
            if (context.Database.IsSqlServer())
            {
                await context.Database.MigrateAsync();
                logger.LogInformation("Database migrated successfully.");
            }

            await SeedAppsAsync(context, logger);
            await SeedUserSubscriptionsAsync(context, logger);
            
            logger.LogInformation("Database initialization completed.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }

    private static async Task SeedAppsAsync(AppsDbContext context, ILogger logger)
    {
        // Seed Apps
        var seedAppsPath = Path.Combine(AppContext.BaseDirectory, "seed-apps.json");
        if (File.Exists(seedAppsPath))
        {
            var jsonApps = await File.ReadAllTextAsync(seedAppsPath);
            // Fix parsing case insensitive
            var opt = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var seedApps = JsonSerializer.Deserialize<List<SeedAppDto>>(jsonApps, opt);

            if (seedApps != null)
            {
                foreach (var seed in seedApps)
                {
                    var existingApp = await context.Apps.FirstOrDefaultAsync(a => a.Id == seed.Id);
                    
                    string dynamicJson = "{}";
                    if (seed.DynamicData != null && seed.DynamicData.Count > 0)
                    {
                        dynamicJson = JsonSerializer.Serialize(seed.DynamicData);
                    }

                    if (existingApp == null)
                    {
                        // Create New
                        var newApp = new AppConfig(seed.Name, seed.description, seed.baseUrl, seed.Id);
                        newApp.UpdateDefaultUserProfile(dynamicJson);
                        newApp.UpdateExternalAuthProviders(seed.externalAuthProvidersJson ?? "{}");
                        newApp.UpdatePrivacyPolicy(seed.PrivacyPolicy ?? "");
                        newApp.UpdateTermsAndConditions(seed.TermsAndConditions ?? "");
                        
                        // Verification configuration
                        if (seed.VerificationType == 3) // KYC -> Use Both as closest approximation or None for now. Let's use Both.
                            newApp.UpdateVerificationType(VerificationType.Both);
                        else if (seed.VerificationType == 2) // Mobile -> Phone
                            newApp.UpdateVerificationType(VerificationType.Phone);
                        else if (seed.VerificationType == 1) // Email
                            newApp.UpdateVerificationType(VerificationType.Email);
                            
                        if (seed.RequiresAdminApproval)
                            newApp.UpdateRequirements(true);
                            
                        if (!seed.IsActive)
                            newApp.Deactivate();

                        await context.Apps.AddAsync(newApp);
                        logger.LogInformation("Seeded App: {Name}", seed.Name);
                    }
                    else
                    {
                        // Update specific fields if needed
                        if (string.IsNullOrEmpty(existingApp.DefaultUserProfileJson) || existingApp.DefaultUserProfileJson == "{}")
                        {
                            if (dynamicJson != "{}")
                            {
                                existingApp.UpdateDefaultUserProfile(dynamicJson);
                                logger.LogInformation("Updated App {Name} with DefaultUserProfileJson", seed.Name);
                            }
                        }

                        if (string.IsNullOrEmpty(existingApp.PrivacyPolicy))
                        {
                            existingApp.UpdatePrivacyPolicy(seed.PrivacyPolicy ?? "");
                            logger.LogInformation("Updated App {Name} with PrivacyPolicy", seed.Name);
                        }

                        if (string.IsNullOrEmpty(existingApp.TermsAndConditions))
                        {
                            existingApp.UpdateTermsAndConditions(seed.TermsAndConditions ?? "");
                            logger.LogInformation("Updated App {Name} with TermsAndConditions", seed.Name);
                        }
                    }
                }
                await context.SaveChangesAsync();
            }
        }

        // Seed Packages
        var seedPackagesPath = Path.Combine(AppContext.BaseDirectory, "seed-packages.json");
        if (File.Exists(seedPackagesPath))
        {
            var jsonPackages = await File.ReadAllTextAsync(seedPackagesPath);
            var opt = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var seedPackages = JsonSerializer.Deserialize<List<SeedPackageDto>>(jsonPackages, opt);
            
            if (seedPackages != null)
            {
                var seededPackageKeys = new HashSet<string>();

                foreach (var sp in seedPackages)
                {
                    seededPackageKeys.Add($"{sp.AppId}_{sp.Name}");

                    // Check if package exists for this App by Name
                    var existingPkg = await context.SubscriptionPackages
                        .FirstOrDefaultAsync(p => p.AppId == sp.AppId && p.Name == sp.Name);
                    
                    if (existingPkg == null)
                    {
                        var pkg = new SubscriptionPackage(sp.AppId, sp.Name, sp.Description, sp.Discount, (SubscriptionPeriod)sp.Period, (PackageType)sp.Type, sp.CoinsAmount, sp.LocalizedPricingJson);
                        if (sp.isActive) pkg.Activate(); else pkg.Deactivate();

                        await context.SubscriptionPackages.AddAsync(pkg);
                        logger.LogInformation("Seeded Package: {Name} for App {AppId}", sp.Name, sp.AppId);
                    }
                    else
                    {
                        if (sp.isActive && !existingPkg.IsActive) existingPkg.Activate();
                        else if (!sp.isActive && existingPkg.IsActive) existingPkg.Deactivate();

                        // Sync dynamic properties
                        existingPkg.UpdatePricing(sp.LocalizedPricingJson);
                    }
                }
                
                // Cleanup unseeded packages
                var allPackages = await context.SubscriptionPackages.ToListAsync();
                foreach (var dbPkg in allPackages)
                {
                    if (!seededPackageKeys.Contains($"{dbPkg.AppId}_{dbPkg.Name}"))
                    {
                        context.SubscriptionPackages.Remove(dbPkg);
                        logger.LogInformation("Removed unseeded Package: {Name} for App {AppId}", dbPkg.Name, dbPkg.AppId);
                    }
                }

                await context.SaveChangesAsync();
            }
        }
    }

    private static decimal GetDefaultPrice(SubscriptionPackage pkg)
    {
        try 
        {
            if (!string.IsNullOrWhiteSpace(pkg.LocalizedPricingJson) && pkg.LocalizedPricingJson != "{}")
            {
                using var doc = System.Text.Json.JsonDocument.Parse(pkg.LocalizedPricingJson);
                if (doc.RootElement.TryGetProperty("Default", out var def) && def.TryGetProperty("Price", out var p))
                {
                    decimal price = p.GetDecimal() - pkg.Discount;
                    return price > 0 ? price : 0m;
                }
            }
        }
        catch { }
        return 0m;
    }

    private static async Task SeedUserSubscriptionsAsync(AppsDbContext context, ILogger logger)
    {
        var wisslerAppId = Guid.Parse("00000000-0000-0000-0000-000000000012");

        // Check if subscriptions exist
        if (await context.UserSubscriptions.AnyAsync(x => x.AppId == wisslerAppId))
        {
            return;
        }

        var packages = await context.SubscriptionPackages.Where(p => p.AppId == wisslerAppId && p.Type == PackageType.Subscription).ToListAsync();
        if (packages.Count == 0) return;

        var weekly = packages.FirstOrDefault(p => p.Name.Contains("Weekly"));
        var monthly = packages.FirstOrDefault(p => p.Name.Contains("Monthly") && !p.Name.Contains("Bi"));
        var unlimited = packages.FirstOrDefault(p => p.Name.Contains("Unlimited"));

        var coins = packages.FirstOrDefault(p => p.Type == PackageType.Consumable);

        if (weekly == null || monthly == null || unlimited == null) return;

        logger.LogInformation("Seeding Egyptian User Subscription histories for Wissler...");

        decimal weeklyPrice = GetDefaultPrice(weekly);
        decimal monthlyPrice = GetDefaultPrice(monthly);
        decimal unlimitedPrice = GetDefaultPrice(unlimited);
        decimal coinsPrice = coins != null ? GetDefaultPrice(coins) : 0m;

        // vis1 to vis5 have histories
        for (int i = 1; i <= 5; i++)
        {
            var userId = Guid.Parse($"00000000-0000-0000-0000-000000000{i:03d}");
            
            // Historical expired package (3 months ago)
            var olderSub = new UserSubscription(userId, wisslerAppId, monthly.Id, DateTime.UtcNow.AddDays(-90), DateTime.UtcNow.AddDays(-60), monthlyPrice, "{}");
            olderSub.ExpireNow();
            await context.UserSubscriptions.AddAsync(olderSub);

            // Historical expired package
            var pastSub = new UserSubscription(userId, wisslerAppId, weekly.Id, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow.AddDays(-23), weeklyPrice, "{}");
            pastSub.ExpireNow();
            await context.UserSubscriptions.AddAsync(pastSub);

            if (coins != null)
            {
                // Buy coins package historically
                var coinsSub = new UserSubscription(userId, wisslerAppId, coins.Id, DateTime.UtcNow.AddDays(-15), DateTime.UtcNow.AddDays(-15), coinsPrice, "{}");
                coinsSub.ExpireNow();
                await context.UserSubscriptions.AddAsync(coinsSub);
            }

            // Active package
            var activeSub = new UserSubscription(userId, wisslerAppId, monthly.Id, DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(28), monthlyPrice, "{}");
            await context.UserSubscriptions.AddAsync(activeSub);
        }

        // vis6 has Unlimited
        var u6 = Guid.Parse("00000000-0000-0000-0000-000000000006");
        var subUnlimited = new UserSubscription(u6, wisslerAppId, unlimited.Id, DateTime.UtcNow.AddDays(-10), DateTime.UtcNow.AddDays(9999), unlimitedPrice, "{}");
        await context.UserSubscriptions.AddAsync(subUnlimited);

        await context.SaveChangesAsync();
    }
    private class SeedAppDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public string baseUrl { get; set; } = string.Empty;
        public string externalAuthProvidersJson { get; set; } = "[]";
        public string PrivacyPolicy { get; set; } = string.Empty;
        public string TermsAndConditions { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int VerificationType { get; set; }
        public bool RequiresAdminApproval { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement> DynamicData { get; set; }
    }

    private class SeedPackageDto
    {
        public Guid AppId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public int Period { get; set; }
        public bool isActive { get; set; }
        public int Type { get; set; }
        public int CoinsAmount { get; set; }
        public string LocalizedPricingJson { get; set; } = "{}";
    }
}
