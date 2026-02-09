using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Users.Domain.Entities;

namespace Users.Infrastructure.Persistence
{
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
                var seedUsers = JsonSerializer.Deserialize<List<SeedUserDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (seedUsers == null) return;

                var random = new Random();
                var genders = new[] { "Male", "Female", "Non-Binary", "Prefer Not to Say" };
                
                // Mock Photos for Seeding
                var mockPhotos = new[] 
                {
                    "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=500&auto=format&fit=crop&q=60",
                    "https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=500&auto=format&fit=crop&q=60",
                    "https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=500&auto=format&fit=crop&q=60",
                    "https://images.unsplash.com/photo-1506794778202-cad84cf45f1d?w=500&auto=format&fit=crop&q=60",
                    "https://images.unsplash.com/photo-1517841905240-472988babdf9?w=500&auto=format&fit=crop&q=60",
                    "https://images.unsplash.com/photo-1539571696357-5a69c17a67c6?w=500&auto=format&fit=crop&q=60",
                    "https://images.unsplash.com/photo-1524504388940-b1c1722653e1?w=500&auto=format&fit=crop&q=60",
                    "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=500&auto=format&fit=crop&q=60",
                    "https://images.unsplash.com/photo-1529626455594-4ff0802cfb7e?w=500&auto=format&fit=crop&q=60",
                    "https://images.unsplash.com/photo-1488426862026-3ee34a7d66df?w=500&auto=format&fit=crop&q=60"
                };

                foreach (var user in seedUsers)
                {
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

                    var globalAppId = Guid.Parse("00000000-0000-0000-0000-000000000001");
                    if (!targetApps.Contains(globalAppId))
                    {
                        targetApps.Add(globalAppId);
                    }

                    foreach (var appId in targetApps)
                    {
                        var displayName = DeriveDisplayName(user.Email);
                        var bio = $"Automated profile for {user.Email}";
                        var gender = genders[random.Next(genders.Length)];
                        var dob = DateTime.UtcNow.AddYears(-random.Next(18, 60)).AddDays(-random.Next(0, 365));
                        
                        // Assign random photos for Wissler users
                        List<string>? userPhotos = null;
                        if (appId == Guid.Parse("22222222-2222-2222-2222-222222222222"))
                        {
                            userPhotos = mockPhotos.OrderBy(x => random.Next()).Take(random.Next(3, 6)).ToList();
                        }

                         await SeedProfileAsync(user.Id, appId, displayName, bio, dob, gender, user.Phone, userPhotos);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "An error occurred while initializing the database (EnsureCreated). Continuing...");
            }
        }

        private string DeriveDisplayName(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return "User";
            var parts = email.Split('@');
            if (parts.Length > 0)
            {
                var name = parts[0];
                if (name.Contains('.'))
                {
                   var subParts = name.Split('.');
                   return string.Join(" ", subParts.Select(p => char.ToUpper(p[0]) + p.Substring(1)));
                }
                return char.ToUpper(name[0]) + name.Substring(1);
            }
            return "User";
        }

        private async Task SeedProfileAsync(Guid userId, Guid appId, string displayName, string bio, DateTime dob, string gender, string? phone, List<string>? photos = null)
        {
            var exists = await _context.UserProfiles.AnyAsync(x => x.UserId == userId && x.AppId == appId);
            if (!exists)
            {
                 var defaults = "{}";
                 try 
                 {
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

                 // Merge Phone and Photos into Defaults/CustomData
                 try 
                 {
                     var jsonNode = JsonNode.Parse(defaults);
                     var jsonObject = jsonNode?.AsObject() ?? new JsonObject();
                     
                     if (!string.IsNullOrEmpty(phone))
                     {
                         jsonObject["mobile"] = phone;
                     }
                     
                     if (photos != null && photos.Any())
                     {
                         var photoArray = new JsonArray();
                         foreach(var p in photos) photoArray.Add(p);
                         jsonObject["photos"] = photoArray;
                     }

                     defaults = jsonObject.ToJsonString();
                 }
                 catch { /* ignore json parse error */ }

                 await _context.Database.ExecuteSqlRawAsync(@"
                    INSERT INTO [UserProfiles] ([Id], [UserId], [AppId], [DisplayName], [Bio], [AvatarUrl], [CustomDataJson], [DateOfBirth], [Gender], [Created])
                    VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9})
                ", 
                Guid.NewGuid(), userId, appId, displayName, bio, photos?.FirstOrDefault(), defaults, dob, gender, DateTime.UtcNow);
                
                _logger.LogInformation("Seeded UserProfile: {DisplayName} ({UserId}) for App {AppId}", displayName, userId, appId);
            }
        }

        private class SeedUserDto
        {
            public Guid Id { get; set; }
            public string Email { get; set; } = string.Empty;
            public string? Phone { get; set; }
        }
    }
}
