using System.Text.Json;
using Auth.Domain.Entities;
using Auth.Domain.Repositories;
using Auth.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Persistence;

public class AuthDbInitializer
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuthDbInitializer> _logger;

    public AuthDbInitializer(IServiceProvider serviceProvider, ILogger<AuthDbInitializer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        int maxRetries = 10;
        int delaySeconds = 2;
        
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                await context.Database.MigrateAsync();
                break; // Success
            }
            catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 1801)
            {
                 _logger.LogWarning(ex, "Database already exists (Error 1801). Attempting to proceed.");
                 // This error typically means "Database already exists". 
                 // We might need to try running migrations again if the first attempt failed during creation but before migration application?
                 // Usually, if 1801 happens, the DB is there, so we can retry the migration action.
                 try 
                 {
                     await context.Database.MigrateAsync();
                     break;
                 }
                 catch (Exception retryEx)
                 {
                      _logger.LogWarning(retryEx, "Retry of MigrateAsync failed after 1801. Assuming resolved or manual intervention needed.");
                      break; // Assume we can proceed if it was just "already exists"
                 }
            }
            catch (Exception ex)
            {
                if (i == maxRetries - 1)
                {
                    _logger.LogError(ex, "MigrateAsync failed after {MaxRetries} attempts. Crashing.", maxRetries);
                    throw;
                }

                _logger.LogWarning(ex, "MigrateAsync failed (Attempt {Attempt}/{MaxRetries}). Retrying in {Delay}s...", i + 1, maxRetries, delaySeconds);
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                delaySeconds = Math.Min(delaySeconds * 2, 30); // Exponential backoff up to 30s
            }
        }

        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        // Check if any users exist
        // Note: We need a method to check count or existence, or just TryGet by email.
        // For simplicity, we assume if we can't find the admin, we create it.
        
        var seedDataPath = Path.Combine(AppContext.BaseDirectory, "seed-users.json");
        if (!File.Exists(seedDataPath))
        {
            _logger.LogWarning("Seed data file not found at {Path}", seedDataPath);
            return;
        }

        // Seed Permissions and Roles
        var accessAllPermission = await context.Permissions.FirstOrDefaultAsync(p => p.Name == "AccessAll");
        if (accessAllPermission == null)
        {
            accessAllPermission = new Permission("AccessAll", "Grant full access to the system");
            context.Permissions.Add(accessAllPermission);
            await context.SaveChangesAsync();
        }

        // System App ID (Hardcoded for Global Admin purposes)
        var systemAppId = Guid.Parse("00000000-0000-0000-0000-000000000001"); 

        var superAdminRole = await context.Roles.Include(r => r.Permissions).FirstOrDefaultAsync(r => r.Name == "SuperAdmin" && r.AppId == systemAppId);
        if (superAdminRole == null)
        {
            superAdminRole = new Role(systemAppId, "SuperAdmin");
            superAdminRole.AddPermission(accessAllPermission);
            superAdminRole.MarkAsSealed();
            context.Roles.Add(superAdminRole);
            await context.SaveChangesAsync();
        }

        var json = await File.ReadAllTextAsync(seedDataPath);
        var seedUsers = JsonSerializer.Deserialize<List<SeedUserDto>>(json);

        if (seedUsers == null) return;

        foreach (var seedUser in seedUsers)
        {
            var user = await userRepository.GetByEmailAsync(seedUser.Email);
            if (user == null)
            {
                var hashedPassword = passwordHasher.Hash(seedUser.Password);
                
                if (seedUser.Id.HasValue)
                {
                    user = new User(seedUser.Id.Value, seedUser.Email, seedUser.Phone, hashedPassword);
                }
                else
                {
                    user = new User(seedUser.Email, seedUser.Phone, hashedPassword);
                }
                
                if (seedUser.IsSealed)
                {
                    user.MarkAsSealed();
                }

                if (seedUser.Status > 0)
                {
                    user.SetStatus((GlobalUserStatus)seedUser.Status);
                }

                if (seedUser.IsEmailVerified) user.VerifyEmail();
                if (seedUser.IsPhoneVerified) user.VerifyPhone();
                if (seedUser.OtpBlockedUntil.HasValue) user.BlockOtp(seedUser.OtpBlockedUntil.Value);

                await userRepository.AddAsync(user);
                _logger.LogInformation("Seeded user: {Email}", seedUser.Email);
            }
            else 
            {
               // If user exists, we might want to ensure they have the fixed ID? 
               // Too risky to change ID of existing entity. Just skip.
            }

            // Assign SuperAdmin Role if requested and not present
            if (seedUser.Roles != null && seedUser.Roles.Contains("SuperAdmin"))
            {
                // Check if membership exists
                // Note: user.Memberships might not be loaded if we just got it from AddAsync above or GetByEmailAsync (repository detail dependent).
                // Safest to check DB context directly for existence.
                var exists = await context.UserAppMemberships.AnyAsync(m => m.UserId == user.Id && m.AppId == systemAppId && m.RoleId == superAdminRole.Id);
                if (!exists)
                {
                    var membership = new UserAppMembership(user.Id, systemAppId, superAdminRole.Id);
                    // We can add via context or user.
                    // If user is tracked, user.AddMembership might work if collection loaded.
                    // Safest: Add to context directly.
                    context.UserAppMemberships.Add(membership);
                    await context.SaveChangesAsync();
                    _logger.LogInformation("Assigned SuperAdmin role to user: {Email}", seedUser.Email);
                }
            }
        }
    }

    private class SeedUserDto
    {
        public Guid? Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public List<string> Roles { get; set; }
        public bool IsSealed { get; set; }
        public int Status { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsPhoneVerified { get; set; }
        public DateTime? OtpBlockedUntil { get; set; }
    }
}
