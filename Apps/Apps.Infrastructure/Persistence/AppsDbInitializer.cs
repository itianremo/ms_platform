using Apps.Domain.Entities;

namespace Apps.Infrastructure.Persistence;

public static class AppsDbInitializer
{
    public static async Task SeedAsync(AppsDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (!context.Apps.Any())
        {
            var fitIt = new AppConfig("FitIT", "Sports and Fitness Application", "http://localhost:5173/app?tenant=fitit");
            fitIt.UpdateTheme("default");
            
            var wissler = new AppConfig("wissler", "Dating and Social Application", "http://localhost:5173/app?tenant=wissler");
            wissler.UpdateTheme("dating-theme");
            
            // Add extra demo app
            var demo = new AppConfig("DemoApp", "Generic Demo App", "http://localhost:5173/app?tenant=demo");
            demo.UpdateTheme("dark");

            await context.Apps.AddRangeAsync(fitIt, wissler, demo);
            await context.SaveChangesAsync();
        }
    }
}
