using System.Text.Json;
using Auth.Domain.Entities;
using Auth.Domain.Repositories;
using Auth.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
        await context.Database.EnsureCreatedAsync();

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

        var json = await File.ReadAllTextAsync(seedDataPath);
        var seedUsers = JsonSerializer.Deserialize<List<SeedUserDto>>(json);

        if (seedUsers == null) return;

        foreach (var seedUser in seedUsers)
        {
            if (await userRepository.GetByEmailAsync(seedUser.Email) == null)
            {
                var hashedPassword = passwordHasher.Hash(seedUser.Password);
                var user = new User(seedUser.Email, seedUser.Phone, hashedPassword, isGlobalAdmin: true); // Assume seeded users are admins for now
                
                if (seedUser.IsSealed)
                {
                    user.MarkAsSealed();
                }

                await userRepository.AddAsync(user);
                _logger.LogInformation("Seeded user: {Email}", seedUser.Email);
            }
        }
    }

    private class SeedUserDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public List<string> Roles { get; set; }
        public bool IsSealed { get; set; }
    }
}
