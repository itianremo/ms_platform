using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Users.Domain.Repositories;

namespace Users.Infrastructure.Persistence;

public class UsersDbInitializer
{
    private readonly UsersDbContext _context;
    private readonly ILogger<UsersDbInitializer> _logger;

    public UsersDbInitializer(UsersDbContext context, ILogger<UsersDbInitializer> logger)
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

            var seedDataPath = Path.Combine(AppContext.BaseDirectory, "seed-users.json");
            if (!File.Exists(seedDataPath))
            {
                _logger.LogWarning("Seed users file not found at {Path}", seedDataPath);
                return;
            }

            var json = await File.ReadAllTextAsync(seedDataPath);
            var seedUsers = System.Text.Json.JsonSerializer.Deserialize<List<SeedUserDto>>(json); // Define DTO inside

            if (seedUsers == null) return;

            var random = new Random();
            var genders = new[] { "Male", "Female", "Non-Binary", "Prefer Not to Say" };

            foreach (var user in seedUsers)
            {
                // Determine App Contexts based on Email Logic
                var targetApps = new List<Guid>();
                var email = user.Email.ToLower();

                // 1. Global / System
                if (email.Contains("@globaldashboard.com") || email.Contains("@global.com"))
                {
                    targetApps.Add(Guid.Parse("00000000-0000-0000-0000-000000000001")); 
                }

                // 2. FitIT
                if (email.Contains("@fitit.com") || email.Contains("@global.com"))
                {
                     targetApps.Add(Guid.Parse("11111111-1111-1111-1111-111111111111"));
                }

                // 3. Wissler
                if (email.Contains("@wissler.com") || email.Contains("@global.com"))
                {
                     targetApps.Add(Guid.Parse("22222222-2222-2222-2222-222222222222"));
                }

                // Fallback: Ensure everyone has a Global Profile
                var globalAppId = Guid.Parse("00000000-0000-0000-0000-000000000001");
                if (!targetApps.Contains(globalAppId))
                {
                    targetApps.Add(globalAppId);
                }

                foreach (var appId in targetApps)
                {
                    // Generate Random Profile Data
                    var displayName = DeriveDisplayName(user.Email, appId);
                    var bio = $"Automated profile for {user.Email}";
                    var gender = genders[random.Next(genders.Length)];
                    var dob = DateTime.UtcNow.AddYears(-random.Next(18, 60)).AddDays(-random.Next(0, 365));

                     await SeedProfileAsync(user.Id, appId, displayName, bio, dob, gender);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "An error occurred while initializing the database (EnsureCreated). Continuing...");
        }
    }

    private string DeriveDisplayName(string email, Guid appId)
    {
        var name = email.Split('@')[0];
        // Capitalize
        name = char.ToUpper(name[0]) + name.Substring(1);
        
        if (appId == Guid.Empty) return $"{name} (Global)";
        // Suffix if needed? Nah, name is fine. 
        // Or specific overrides if matching hardcoded expectations?
        // Logic asked for "Assumptions", so derived is fine.
        return name;
    }

    private async Task SeedProfileAsync(Guid userId, Guid appId, string displayName, string bio, DateTime dob, string gender)
    {
        var exists = await _context.UserProfiles.AnyAsync(x => x.UserId == userId && x.AppId == appId);
        if (!exists)
        {
             // Get Defaults directly from AppsDb to avoid dependency issues during migration/startup
             var defaults = "{}";
             try 
             {
                 // Only verify AppsDb connection if not Empty/Global (Global might not have entries in Apps table if it's ID 000...01 but we use Empty here)
                 // AppId Empty usually implies 'System' or 'No App'.
                 if (appId != Guid.Empty) 
                 {
                     var sql = "SELECT DefaultUserProfileJson FROM [AppsDb].[dbo].[Apps] WHERE Id = {0}";
                     var result = await _context.Database.SqlQueryRaw<string>(sql, appId).ToListAsync();
                     defaults = result.FirstOrDefault() ?? "{}";
                 }
             }
             catch
             {
                 defaults = "{}";
             }

             await _context.Database.ExecuteSqlRawAsync(@"
                INSERT INTO [UserProfiles] ([Id], [UserId], [AppId], [DisplayName], [Bio], [AvatarUrl], [CustomDataJson], [DateOfBirth], [Gender], [Created])
                VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9})
            ", 
            Guid.NewGuid(), userId, appId, displayName, bio, null, defaults, dob, gender, DateTime.UtcNow);
            
            _logger.LogInformation("Seeded UserProfile: {DisplayName} ({UserId}) for App {AppId}", displayName, userId, appId);
        }
    }

    private class SeedUserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
    }
}

