using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
                // Manual Patch for Schema Drift (EnsureCreated vs Migrations conflict)
                // Manual Patch for Schema Drift (EnsureCreated vs Migrations conflict)
                // Removed legacy patches

                await _context.Database.MigrateAsync();
            }

            // Seed Admin Profile
            var adminUserId = Guid.Parse("cabbfc49-4488-47d1-9379-33dd46049139");
            var adminProfileExists = await _context.UserProfiles.AnyAsync(x => x.UserId == adminUserId);
            
            if (!adminProfileExists)
            {
                // Using SQL for simplicity and robustness against missing imports in this snippet
                await _context.Database.ExecuteSqlRawAsync(@"
                    INSERT INTO [UserProfiles] ([Id], [UserId], [AppId], [DisplayName], [Bio], [AvatarUrl], [CustomDataJson], [DateOfBirth], [Gender])
                    VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8})
                ", 
                Guid.NewGuid(), // Profile Id
                adminUserId, // UserId
                Guid.Empty, // AppId (System / Global) -> 00000000-0000-0000-0000-000000000000
                "System Admin", // DisplayName
                "Global Administrator", // Bio
                null, // AvatarUrl
                "{}", // CustomDataJson
                null, // DateOfBirth
                null // Gender
                );
                
                _logger.LogInformation("Seeded UserProfile for Admin: {UserId}", adminUserId);
            }
        }
        catch (Exception ex)
        {
            // Ignore (logging as warning) if database already exists or other ensure-created issues, 
            // so the service can proceed to run.
            _logger.LogWarning(ex, "An error occurred while initializing the database (EnsureCreated). Continuing...");
        }
    }
}
