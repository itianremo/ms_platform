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

                await SeedReportReasonsAsync();
                await SyncRoleIdsFromAuthDbAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "An error occurred while initializing the database (EnsureCreated). Continuing...");
            }
        }

        private async Task SeedBasicUsersAsync(List<SeedUserDto> seedUsers)
        {
            var random = new Random();
            var genders = new[] { "Male", "Female" };
            var jobs = new[] { "Software Engineer", "Designer", "Chef", "Doctor", "Teacher", "Writer", "Musician" };
            var educations = new[] { "High School", "Bachelors", "Masters", "PhD" };
            var drinkSmoking = new[] { "Never", "Socially", "Often" };
            var eyeColors = new[] { "Blue", "Brown", "Green", "Hazel" };
            var hairColors = new[] { "Blonde", "Brown", "Black", "Red", "Grey" };
            var interestedIns = new[] { "Male", "Female", "All" };
            var egyptCities = new[] { "Cairo", "Alexandria", "Giza", "Sharm El-Sheikh", "Luxor", "Aswan", "Hurghada", "Mansoura" };
            
            var mockPhotos = GetMockPhotos();
            
            var globalAdminAppId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var wisslerAdminAppId = Guid.Parse("00000000-0000-0000-0000-000000000002");
            var wisslerAppId = Guid.Parse("00000000-0000-0000-0000-000000000012");
            var fititAppId = Guid.Parse("00000000-0000-0000-0000-000000000013");

            // Cleanup any users not in the seed array
            var allDbProfiles = await _context.UserProfiles.ToListAsync();
            var seedUserIds = seedUsers.Select(x => x.Id).ToHashSet();
            foreach (var profile in allDbProfiles)
            {
                if (!seedUserIds.Contains(profile.UserId))
                {
                    _context.UserProfiles.Remove(profile);
                    _logger.LogInformation("Removed unseeded profile for User: {UserId}", profile.UserId);
                }
            }
            await _context.SaveChangesAsync();

            foreach (var user in seedUsers)
            {
                var targetApps = new List<Guid>();
                var email = user.Email.ToLower();

                if (email.Contains("admin@ump.com")) 
                {
                    // Superadmin gets profiles everywhere
                    targetApps.Add(globalAdminAppId);
                    targetApps.Add(wisslerAdminAppId);
                    targetApps.Add(wisslerAppId);
                    targetApps.Add(fititAppId);
                }
                else if (email.Contains("@wissler.com")) 
                {
                    if (email.StartsWith("admin") || email.StartsWith("manager"))
                        targetApps.Add(wisslerAdminAppId);
                    else if (email.StartsWith("vis15"))
                    {
                        targetApps.Add(wisslerAppId);
                        targetApps.Add(fititAppId);
                    }
                    else if (email.StartsWith("vis13") || email.StartsWith("vis14"))
                        targetApps.Add(fititAppId);
                    else
                        targetApps.Add(wisslerAppId); // vis1 to vis12
                }
                else if (email.Contains("@ump.com"))
                {
                   targetApps.Add(globalAdminAppId);
                }

                foreach (var appId in targetApps)
                {
                     // Skip if already exists
                     if (await _context.UserProfiles.AnyAsync(x => x.UserId == user.Id && x.AppId == appId)) continue;

                    var isVisitor = email.StartsWith("vis");
                    var displayName = isVisitor ? $"Visitor {email.Replace("vis", "").Replace("@wissler.com", "")}" : DeriveDisplayName(user.Email);
                    var gender = genders[random.Next(genders.Length)];
                    var dob = DateTime.UtcNow.AddYears(-random.Next(21, 40)).AddDays(-random.Next(0, 365));
                    
                    var customData = new JsonObject();
                    if (!string.IsNullOrEmpty(user.Phone)) customData["mobile"] = user.Phone;
                    
                    if (isVisitor)
                    {
                        // Specific rich Egyptian profile for Visitor
                        var photoUrls = mockPhotos.OrderBy(x => random.Next()).Take(3).ToList();
                        var photoObjects = new JsonArray();
                        foreach (var url in photoUrls)
                        {
                            photoObjects.Add(new JsonObject { ["url"] = url, ["isApproved"] = true, ["isVerified"] = true, ["isActive"] = true });
                        }
                        
                        customData["cityId"] = egyptCities[random.Next(egyptCities.Length)];
                        customData["countryId"] = "Egypt";
                        customData["height"] = random.Next(155, 195);
                        customData["job"] = jobs[random.Next(jobs.Length)];
                        customData["education"] = educations[random.Next(educations.Length)];
                        customData["drinking"] = drinkSmoking[random.Next(drinkSmoking.Length)];
                        customData["smoking"] = drinkSmoking[random.Next(drinkSmoking.Length)];
                        customData["eyeColor"] = eyeColors[random.Next(eyeColors.Length)];
                        customData["hairColor"] = hairColors[random.Next(hairColors.Length)];
                        customData["interestedIn"] = interestedIns[random.Next(interestedIns.Length)];
                        customData["photos"] = photoObjects;
                        customData["interests"] = new JsonArray("Travel", "Music", "Food", "Tech");
                        customData["languages"] = new JsonArray("Arabic", "English");
                        
                        int loyaltyPoints = 0;
                        int coinsBalance = 0;
                        if (user.Email.StartsWith("vis001") || user.Email.StartsWith("vis002") || user.Email.StartsWith("vis003") || user.Email.StartsWith("vis004") || user.Email.StartsWith("vis005"))
                        {
                            loyaltyPoints = 25; // Math approx 10% of total past subscriptions seeded in AppsDb
                            coinsBalance = 100; // If they bought coins bundle
                        }
                        else if (user.Email.StartsWith("vis006"))
                        {
                            loyaltyPoints = 120; // Unlimited package math
                        }

                        var bio = $"Hi! I am from {customData["cityId"]}. Looking forward to connecting!";
                        await SeedProfileRawAsync(user.Id, appId, displayName, bio, photoUrls.First(), customData.ToJsonString(), dob, gender, loyaltyPoints, coinsBalance);
                    }
                    else {
                        // Admin Default Profile
                        var simplePhotos = mockPhotos.OrderBy(x => random.Next()).Take(1).ToList();
                        var bio = $"Automated profile for {user.Email}";
                        await SeedProfileRawAsync(user.Id, appId, displayName, bio, simplePhotos.First(), customData.ToJsonString(), dob, gender, 0, 0);
                    }
                }
            }
        }

        private async Task SeedReportReasonsAsync()
        {
            var wisslerAppId = Guid.Parse("00000000-0000-0000-0000-000000000012");

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

        private async Task SyncRoleIdsFromAuthDbAsync()
        {
            try
            {
                _logger.LogInformation("Synchronizing empty RoleIds in UserProfiles from AuthDb...");
                
                var sql = @"
                    UPDATE up
                    SET RoleId = (SELECT TOP 1 Id FROM [AuthDb].[dbo].[Roles] r WHERE up.AppId = r.AppId AND r.Name = CASE 
                        WHEN up.DisplayName LIKE '%Admin%' THEN 'SuperAdmin'
                        WHEN up.DisplayName LIKE '%Manager%' THEN 'ManageUsers'
                        ELSE 'Visitor' 
                    END)
                    FROM [UserProfiles] up
                    WHERE up.RoleId = '00000000-0000-0000-0000-000000000000'
                ";
                
                var rowsAffected = await _context.Database.ExecuteSqlRawAsync(sql);
                if (rowsAffected > 0)
                {
                    _logger.LogInformation("Successfully synchronized {Count} RoleIds for UserProfiles.", rowsAffected);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to synchronize RoleIds from AuthDb. Skipping for now.");
            }
        }

        private async Task SeedProfileRawAsync(Guid userId, Guid appId, string displayName, string bio, string avatarUrl, string customDataJson, DateTime dob, string gender, int loyaltyPoints = 0, int coinsBalance = 0)
        {
             // Direct SQL Insert to bypass EF complexities for seeding
             await _context.Database.ExecuteSqlRawAsync(@"
                INSERT INTO [UserProfiles] ([Id], [UserId], [AppId], [DisplayName], [Bio], [AvatarUrl], [CustomDataJson], [DateOfBirth], [Gender], [Created], [LoyaltyPoints], [CoinsBalance])
                VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11})
            ", 
            Guid.NewGuid(), userId, appId, displayName, bio, avatarUrl, customDataJson, dob, gender, DateTime.UtcNow, loyaltyPoints, coinsBalance);
            
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
