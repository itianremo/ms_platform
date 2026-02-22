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
                if (File.Exists(seedDataPath))
                {
                    var json = await File.ReadAllTextAsync(seedDataPath);
                    var seedUsers = JsonSerializer.Deserialize<List<SeedUserDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (seedUsers != null)
                    {
                         await SeedBasicUsersAsync(seedUsers);
                    }
                }

                // --- NEW: Seed Report Reasons ---
                await SeedReportReasonsAsync();

                // --- NEW: Seed 5 Specific "Visitor" Users for Wissler (Requested minimum) ---
                await SeedWisslerVisitorsAsync();
                // ----------------------------------------------------------
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "An error occurred while initializing the database (EnsureCreated). Continuing...");
            }
        }

        private async Task SeedBasicUsersAsync(List<SeedUserDto> seedUsers)
        {
            var random = new Random();
            var genders = new[] { "Male", "Female", "Non-Binary", "Prefer Not to Say" };
            var interestedIns = new[] { "Male", "Female", "All" };
            var mockPhotos = GetMockPhotos();
            var wisslerAppId = Guid.Parse("22222222-2222-2222-2222-222222222222");

            foreach (var user in seedUsers)
            {
                var targetApps = new List<Guid>();
                var email = user.Email.ToLower();

                if (email.Contains("@globaldashboard.com") || email.Contains("@global.com")) targetApps.Add(Guid.Parse("00000000-0000-0000-0000-000000000001"));
                if (email.Contains("@fitit.com") || email.Contains("@global.com")) targetApps.Add(Guid.Parse("11111111-1111-1111-1111-111111111111"));
                if (email.Contains("@wissler.com") || email.Contains("@global.com")) targetApps.Add(wisslerAppId);

                var globalAppId = Guid.Parse("00000000-0000-0000-0000-000000000001");
                if (!targetApps.Contains(globalAppId)) targetApps.Add(globalAppId);

                foreach (var appId in targetApps)
                {
                     // Skip if already exists
                     if (await _context.UserProfiles.AnyAsync(x => x.UserId == user.Id && x.AppId == appId)) continue;

                    var displayName = DeriveDisplayName(user.Email);
                    var gender = genders[random.Next(genders.Length)];
                    var dob = DateTime.UtcNow.AddYears(-random.Next(25, 45)).AddDays(-random.Next(0, 365));
                    
                    var customData = new JsonObject();
                    if (!string.IsNullOrEmpty(user.Phone)) customData["mobile"] = user.Phone;
                    
                    if (appId == wisslerAppId && email == "admin@globaldashboard.com")
                    {
                        // Specific rich profile for Admin in Wissler
                        var photoUrls = mockPhotos.OrderBy(x => random.Next()).Take(4).ToList();
                        var photoObjects = new JsonArray();
                        foreach (var url in photoUrls)
                        {
                            photoObjects.Add(new JsonObject
                            {
                                ["url"] = url,
                                ["isApproved"] = true,
                                ["isVerified"] = true,
                                ["isActive"] = true
                            });
                        }
                        
                        customData["cityId"] = "Admin City";
                        customData["countryId"] = "Admin Country";
                        customData["height"] = 185;
                        customData["job"] = "System Administrator";
                        customData["education"] = "Masters";
                        customData["interestedIn"] = "All";
                        customData["emailVerified"] = true;
                        customData["phoneVerified"] = true;
                        customData["photos"] = photoObjects;
                        customData["interests"] = new JsonArray("Tech", "Management", "Coffee");
                        customData["settings"] = new JsonObject { ["pushNotifications"] = true, ["privacy"] = "public" };
                        customData["filters"] = new JsonObject { ["maxDistance"] = 50, ["ageRange"] = new JsonArray(18, 50) };
                        
                        var adminBio = "I am the platform administrator testing the Wissler application features.";
                        await SeedProfileRawAsync(user.Id, appId, displayName, adminBio, photoUrls.First(), customData.ToJsonString(), dob, gender);
                    }
                    else
                    {
                        // Standard basic fallback behavior
                        var simplePhotos = mockPhotos.OrderBy(x => random.Next()).Take(3).ToList();
                        var photoArray = new JsonArray();
                        foreach(var p in simplePhotos) photoArray.Add(p);
                        customData["Images"] = photoArray;
                        var bio = $"Automated profile for {user.Email}";
                        
                        await SeedProfileRawAsync(user.Id, appId, displayName, bio, simplePhotos.First(), customData.ToJsonString(), dob, gender);
                    }
                }
            }
        }

        private async Task SeedReportReasonsAsync()
        {
            var wisslerAppId = Guid.Parse("22222222-2222-2222-2222-222222222222");

            if (!await _context.ReportReasons.AnyAsync(r => r.AppId == wisslerAppId))
            {
                var reasons = new List<ReportReason>
                {
                    new ReportReason { AppId = wisslerAppId, ReasonText = "Nothing", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new ReportReason { AppId = wisslerAppId, ReasonText = "Inappropriate Content", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new ReportReason { AppId = wisslerAppId, ReasonText = "Spam or Scam", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new ReportReason { AppId = wisslerAppId, ReasonText = "Fake Profile", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new ReportReason { AppId = wisslerAppId, ReasonText = "Harassment or Hostile Behavior", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new ReportReason { AppId = wisslerAppId, ReasonText = "Underage User", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new ReportReason { AppId = wisslerAppId, ReasonText = "Other", IsActive = true, CreatedAt = DateTime.UtcNow }
                };

                await _context.ReportReasons.AddRangeAsync(reasons);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Seeded standard report reasons for Wissler.");
            }
        }

        private async Task SeedWisslerVisitorsAsync()
        {
            var wisslerAppId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            
            _logger.LogInformation("Seeding 5 Realistic Visitor Users for Wissler...");

            var random = new Random();
            var genders = new[] { "Male", "Female" };
            var jobs = new[] { "Software Engineer", "Designer", "Chef", "Doctor", "Teacher", "Writer", "Musician" };
            var educations = new[] { "High School", "Bachelors", "Masters", "PhD" };
            var drinkSmoking = new[] { "Never", "Socially", "Often" };
            var eyeColors = new[] { "Blue", "Brown", "Green", "Hazel" };
            var hairColors = new[] { "Blonde", "Brown", "Black", "Red", "Grey" };
            var interestedIns = new[] { "Male", "Female", "All" };

            var mockPhotos = GetMockPhotos();

            // Just 5 users as requested
            for (int i = 1; i <= 5; i++)
            {
                var email = $"vis{i}@wissler.com";
                
                var existingProfile = await _context.UserProfiles.FirstOrDefaultAsync(u => u.AppId == wisslerAppId && u.DisplayName == $"Visitor {i}");
                if (existingProfile != null) continue;

                var userId = Guid.NewGuid();
                
                var gender = genders[random.Next(genders.Length)];
                var displayName = $"Visitor {i}";
                var age = random.Next(21, 40);
                var dob = DateTime.UtcNow.AddYears(-age).AddDays(-random.Next(1, 365));
                var job = jobs[random.Next(jobs.Length)];
                var edu = educations[random.Next(educations.Length)];
                var interestedIn = interestedIns[random.Next(interestedIns.Length)];
                
                var photoUrls = mockPhotos.OrderBy(x => random.Next()).Take(3).ToList();
                var photoObjects = new JsonArray();
                
                foreach (var url in photoUrls)
                {
                    photoObjects.Add(new JsonObject
                    {
                        ["url"] = url,
                        ["isApproved"] = true, 
                        ["isVerified"] = true,
                        ["isActive"] = true
                    });
                }
                
                var customData = new JsonObject
                {
                    ["cityId"] = "Berlin",
                    ["countryId"] = "Germany",
                    ["height"] = random.Next(160, 200),
                    ["job"] = job,
                    ["education"] = edu,
                    ["drinking"] = drinkSmoking[random.Next(drinkSmoking.Length)],
                    ["smoking"] = drinkSmoking[random.Next(drinkSmoking.Length)],
                    ["eyeColor"] = eyeColors[random.Next(eyeColors.Length)],
                    ["hairColor"] = hairColors[random.Next(hairColors.Length)],
                    ["interestedIn"] = interestedIn,
                    ["photos"] = photoObjects,
                    ["interests"] = new JsonArray("Travel", "Music", "Food"),
                    ["languages"] = new JsonArray("English", "German")
                };

                var bio = $"Hi, I'm {displayName}. I work as a {job}. Checking out Wissler!";
                
                await SeedProfileRawAsync(userId, wisslerAppId, displayName, bio, photoUrls.First(), customData.ToJsonString(), dob, gender);
            }
        }

        private async Task SeedProfileRawAsync(Guid userId, Guid appId, string displayName, string bio, string avatarUrl, string customDataJson, DateTime dob, string gender)
        {
             // Direct SQL Insert to bypass EF complexities for seeding
             await _context.Database.ExecuteSqlRawAsync(@"
                INSERT INTO [UserProfiles] ([Id], [UserId], [AppId], [DisplayName], [Bio], [AvatarUrl], [CustomDataJson], [DateOfBirth], [Gender], [Created])
                VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9})
            ", 
            Guid.NewGuid(), userId, appId, displayName, bio, avatarUrl, customDataJson, dob, gender, DateTime.UtcNow);
            
            _logger.LogInformation("Seeded User: {Name}", displayName);
        }

        private string DeriveDisplayName(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return "User";
            var parts = email.Split('@');
            return char.ToUpper(parts[0][0]) + parts[0].Substring(1);
        }

        private string[] GetMockPhotos()
        {
            return new[] 
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
        }

        private class SeedUserDto
        {
            public Guid Id { get; set; }
            public string Email { get; set; } = string.Empty;
            public string? Phone { get; set; }
        }
    }
}
