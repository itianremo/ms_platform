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

        // Helper to load JSON
        async Task<T?> LoadJsonAsync<T>(string fileName)
        {
            var path = Path.Combine(AppContext.BaseDirectory, fileName);
            if (!File.Exists(path))
            {
                 _logger.LogWarning("Seed file not found: {Path}", path);
                 return default;
            }
            var text = await File.ReadAllTextAsync(path);
            return JsonSerializer.Deserialize<T>(text);
        }

        // 1. Seed Permissions
        var seedPermissions = await LoadJsonAsync<List<SeedPermissionDto>>("seed-permissions.json");
        if (seedPermissions != null)
        {
            foreach (var sp in seedPermissions)
            {
                var existing = await context.Permissions.FirstOrDefaultAsync(p => p.Name == sp.Name);
                if (existing == null)
                {
                    existing = new Permission(sp.Name, sp.Description);
                    context.Permissions.Add(existing);
                    _logger.LogInformation("Seeded Permission: {Name}", sp.Name);
                }
            }
            await context.SaveChangesAsync();
        }

        // 2. Seed Roles and Link Permissions for ALL Apps
        // Initialize Roles for All Known Apps
        var appIds = new[] 
        {
            Guid.Parse("00000000-0000-0000-0000-000000000001"), // Global Admin
            Guid.Parse("00000000-0000-0000-0000-000000000002"), // Wissler Admin
            Guid.Parse("00000000-0000-0000-0000-000000000003"), // FitIT Admin
            Guid.Parse("00000000-0000-0000-0000-000000000012"), // Wissler App
            Guid.Parse("00000000-0000-0000-0000-000000000013"), // FitIT App
        };
        
        var seedRoles = await LoadJsonAsync<List<SeedRoleDto>>("seed-roles.json");
        
        if (seedRoles != null)
        {
            foreach (var appId in appIds)
            {
                foreach (var sr in seedRoles)
                {
                    var role = await context.Roles
                        .Include(r => r.Permissions)
                        .FirstOrDefaultAsync(r => r.Name == sr.Name && r.AppId == appId);
                        
                    if (role == null)
                    {
                        role = new Role(appId, sr.Name);
                        if (sr.IsSealed) role.MarkAsSealed();
                        context.Roles.Add(role);
                        await context.SaveChangesAsync(); // Save to get ID
                        _logger.LogInformation("Seeded Role: {Role} for App {AppId}", sr.Name, appId);
                    }
                    
                    // Sync Permissions
                    if (sr.Permissions != null)
                    {
                        foreach (var permName in sr.Permissions)
                        {
                            // Avoid duplicates
                            if (role.Permissions.Any(p => p.Name == permName)) continue;

                            var perm = await context.Permissions.FirstOrDefaultAsync(p => p.Name == permName);
                            if (perm != null)
                            {
                                role.AddPermission(perm);
                            }
                        }
                    }
                }
            }
            await context.SaveChangesAsync();
        }

        var json = await File.ReadAllTextAsync(seedDataPath);
        var seedUsers = JsonSerializer.Deserialize<List<SeedUserDto>>(json);

        if (seedUsers == null) return;

        // Cleanup any users not in the seed array
        var allDbUsers = await context.Users.ToListAsync();
        var seedEmails = seedUsers.Select(x => x.Email.ToLower()).ToHashSet();
        foreach (var dbUser in allDbUsers)
        {
            if (!seedEmails.Contains(dbUser.Email.ToLower()))
            {
                context.Users.Remove(dbUser);
                _logger.LogInformation("Removed unseeded user: {Email}", dbUser.Email);
            }
        }
        await context.SaveChangesAsync();

        foreach (var seedUser in seedUsers)
        {
            var user = await userRepository.GetByEmailAsync(seedUser.Email);
            if (user == null && seedUser.Id.HasValue)
            {
                user = await userRepository.GetByIdAsync(seedUser.Id.Value);
            }

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
                // Ensure Password is correct for existing users (Dev/Demo convenience)
                var hashedPassword = passwordHasher.Hash(seedUser.Password);
                // We don't check Verify() here to avoid overhead, just overwrite to ensure known state.
                user.UpdatePassword(hashedPassword);
                
                // Ensure Email matches Seed (fix for historic data mismatch)
                if (!string.Equals(user.Email, seedUser.Email, StringComparison.OrdinalIgnoreCase))
                {
                    user.UpdateEmail(seedUser.Email);
                    _logger.LogInformation("Updated email for user {Id} to {Email}", user.Id, seedUser.Email);
                }

                // Also ensure Status is correct if we want to enforce seed state
                if (seedUser.Status > 0) user.SetStatus((GlobalUserStatus)seedUser.Status);
                if (seedUser.IsEmailVerified) user.VerifyEmail(); // This will re-verify after UpdateEmail set it to false
                if (seedUser.IsPhoneVerified) user.VerifyPhone();

                await userRepository.UpdateAsync(user); // Ensure changes are saved
                _logger.LogInformation("Updated existing seed user: {Email}", seedUser.Email);
            }

            // Bind Roles (Native EF Core Many-to-Many via ms_auth schema, meaning we insert to UserRoles)
            await SyncUserRolesAsync(context, user.Id, seedUser.Email, seedUser.Roles);
        }
    }

    private async Task SyncUserRolesAsync(AuthDbContext context, Guid userId, string email, List<string> seedRoles)
    {
        var globalAdminAppId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var wisslerAdminAppId = Guid.Parse("00000000-0000-0000-0000-000000000002");
        var wisslerAppId = Guid.Parse("00000000-0000-0000-0000-000000000012");
        var fititAppId = Guid.Parse("00000000-0000-0000-0000-000000000013");

        var targetApps = new List<Guid>();

        // Logically map user to Apps based on requirements
        if (email.Contains("admin@ump.com")) 
        {
            targetApps.Add(globalAdminAppId);
        }
        else if (email.StartsWith("admin@wissler.com") || email.StartsWith("manager@wissler.com")) 
        {
            targetApps.Add(wisslerAdminAppId);
        }
        else if (email.StartsWith("vis15"))
        {
            targetApps.Add(wisslerAppId);
            targetApps.Add(fititAppId);
        }
        else if (email.StartsWith("vis13") || email.StartsWith("vis14"))
        {
            targetApps.Add(fititAppId);
        }
        else if (email.StartsWith("vis"))
        {
            targetApps.Add(wisslerAppId);
        }

        // Roles resolution
        string activeRoleName = "Visitor";
        if (email.Contains("admin@ump.com")) activeRoleName = "SuperAdmin";
        else if (email.Contains("admin@wissler.com") || email.Contains("manager@wissler.com")) activeRoleName = "ManageUsers";

        foreach (var appId in targetApps)
        {
            var role = await context.Roles.FirstOrDefaultAsync(r => r.AppId == appId && r.Name == activeRoleName);
            if (role != null)
            {
                // Check if user already has this role in this App
                var sqlCheck = "SELECT COUNT(1) FROM [UserAppMemberships] WHERE [UserId] = {0} AND [RoleId] = {1} AND [AppId] = {2}";
                var count = await context.Database.ExecuteSqlRawAsync(
                    "IF NOT EXISTS (SELECT 1 FROM [UserAppMemberships] WHERE [UserId] = {0} AND [RoleId] = {1} AND [AppId] = {2}) " +
                    "INSERT INTO [UserAppMemberships] ([Id], [UserId], [AppId], [RoleId], [Status]) " +
                    "VALUES (NEWID(), {0}, {2}, {1}, 'Active')", 
                    userId, role.Id, appId);
            }
        }
    }

    private class SeedUserDto
    {
        public Guid? Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public bool IsSealed { get; set; }
        public int Status { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsPhoneVerified { get; set; }
        public DateTime? OtpBlockedUntil { get; set; }
    }

    private class SeedRoleDto
    {
        public string Name { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new();
        public bool IsSealed { get; set; }
    }

    private class SeedPermissionDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
