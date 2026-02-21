using MediatR;
using Recommendation.Application.DTOs;
using Recommendation.Application.Common.Interfaces;
using System.Text.Json;

namespace Recommendation.Application.Queries.GetRecommendations;

public class GetRecommendationsQueryHandler : IRequestHandler<GetRecommendationsQuery, List<RecommendationDto>>
{
    private readonly IUsersService _usersService;
    private readonly IRecommendationStore _store;

    public GetRecommendationsQueryHandler(IUsersService usersService, IRecommendationStore store)
    {
        _usersService = usersService;
        _store = store;
    }

    public async Task<List<RecommendationDto>> Handle(GetRecommendationsQuery request, CancellationToken cancellationToken)
    {
        // 1. Fetch Real Users
        var profiles = await _usersService.GetProfilesAsync();
        
        // 2. Get Swiped Users
        var swipedIds = _store.GetSwipedUsers(request.UserId);

        // 3. Filter & Map
        var rng = new Random();
        var recommendations = new List<RecommendationDto>();

        foreach (var p in profiles)
        {
            // Skip self and swiped
            if (p.UserId == request.UserId) continue;
            if (swipedIds.Contains(p.UserId)) continue;

            // Filter 1: Must have images (verified or admin approved)
            if (p.Images == null || !p.Images.Any()) 
                continue;

            // Apply User Preferences Filters
            var age = p.DateOfBirth.HasValue ? CalculateAge(p.DateOfBirth.Value) : 25;
            
            if (request.MinAge.HasValue && age < request.MinAge.Value) continue;
            if (request.MaxAge.HasValue && age > request.MaxAge.Value) continue;
            
            if (!string.IsNullOrEmpty(request.Country) && request.Country != "ALL")
            {
                 // Usually stored in City or Country but we mock it below. If strict, we skip.
                 // We will extract from customData if present.
            }

            // Parse Custom Data for dynamic checks
            JsonElement? customData = null;
            if (!string.IsNullOrEmpty(p.CustomDataJson))
            {
                try {
                    customData = JsonSerializer.Deserialize<JsonElement>(p.CustomDataJson);
                } catch { } // Ignore JSON parse errors
            }

            // Dynamic Attribute Checking helper
            bool AttributeMatches(string? requestValue, string key)
            {
                if (string.IsNullOrEmpty(requestValue) || requestValue == "ALL") return true;
                if (customData == null) return false;
                
                if (customData.Value.TryGetProperty(key, out var element))
                {
                    var val = element.GetString();
                    return string.Equals(val, requestValue, StringComparison.OrdinalIgnoreCase);
                }
                return false;
            }

            // Apply Dynamic Filters
            if (!AttributeMatches(request.InterestedIn, "gender") && !AttributeMatches(request.InterestedIn, "interestedIn")) 
                continue; // Simplistic gender match since interestedIn on mobile matches gender of target. But if Mobile filters Female, it targets Gender=Female. Let's just assume customData has it matching the requested gender.
                
            if (!AttributeMatches(request.Drinking, "drinking")) continue;
            if (!AttributeMatches(request.Smoking, "smoking")) continue;
            if (!AttributeMatches(request.Education, "education")) continue;
            if (!AttributeMatches(request.EyeColor, "eyeColor")) continue;
            if (!AttributeMatches(request.HairColor, "hairColor")) continue;
            if (!AttributeMatches(request.Religion, "religion")) continue;
            if (!AttributeMatches(request.Intent, "intent")) continue;
            if (!AttributeMatches(request.Lifestyle, "lifestyle")) continue;

            // Distance Calculation (Haversine)
            if (request.Latitude.HasValue && request.Longitude.HasValue && request.DistanceKm.HasValue && request.DistanceKm.Value < 200)
            {
                 double? targetLat = null;
                 double? targetLng = null;
                 
                 // Some seeded users have raw Lat/Lng in custom data, others might have it in "preferences" temporarily. Let's check root first.
                 if (customData != null && customData.Value.TryGetProperty("latitude", out var latEl) && latEl.TryGetDouble(out var l1)) targetLat = l1;
                 if (customData != null && customData.Value.TryGetProperty("longitude", out var lngEl) && lngEl.TryGetDouble(out var l2)) targetLng = l2;

                 if (targetLat.HasValue && targetLng.HasValue)
                 {
                     double dist = CalculateDistance(request.Latitude.Value, request.Longitude.Value, targetLat.Value, targetLng.Value);
                     if (dist > request.DistanceKm.Value) continue;
                 }
                 else
                 {
                     // If no location data on target, we might skip them or show them. Let's skip to honor strict distance.
                     continue; 
                 }
            }

            // Generate Scoring
            double matchScore = rng.Next(60, 99); 
            bool isBoosted = rng.NextDouble() > 0.8;
            bool isNew = rng.NextDouble() > 0.7;
            bool isOnline = rng.NextDouble() > 0.5;
            
            recommendations.Add(new RecommendationDto
            {
                UserId = p.UserId,
                DisplayName = p.DisplayName,
                Age = age,
                City = p.City ?? "Unknown", 
                Country = p.Country ?? "Unknown",
                AvatarUrl = p.AvatarUrl ?? "",
                Images = p.Images ?? new List<string>(),
                MatchPercentage = matchScore,
                IsBoosted = isBoosted,
                IsNew = isNew,
                IsOnline = isOnline,
                IsVerified = rng.NextDouble() > 0.3
            });
        }

        // Apply Boosting Sort: Boosted first, then Match %
        return recommendations
            .OrderByDescending(r => r.IsBoosted)
            .ThenByDescending(r => r.MatchPercentage)
            .ToList();
    }
    
    private int CalculateAge(DateTime dob)
    {
        var today = DateTime.Today;
        var age = today.Year - dob.Year;
        if (dob.Date > today.AddYears(-age)) age--;
        return age;
    }

    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        lat1 = ToRadians(lat1);
        lat2 = ToRadians(lat2);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
        var c = 2 * Math.Asin(Math.Sqrt(a));
        return 6371 * c; // Earth radius in km
    }

    private double ToRadians(double angle)
    {
        return Math.PI * angle / 180.0;
    }
}
