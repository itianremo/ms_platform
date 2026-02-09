using MediatR;
using Recommendation.Application.DTOs;

namespace Recommendation.Application.Queries.GetRecommendations;

public class GetRecommendationsQueryHandler : IRequestHandler<GetRecommendationsQuery, List<RecommendationDto>>
{
    public Task<List<RecommendationDto>> Handle(GetRecommendationsQuery request, CancellationToken cancellationToken)
    {
        // Mock Implementation for MVP / Demo
        // In real world, this would query a Graph DB or Vector DB
        
        var rng = new Random();
        var recommendations = new List<RecommendationDto>();
        
        // Generate flexible dummy data based on seed users we know
        var baseUsers = new[]
        {
            new { Name = "Sarah", Age = 24, Country = "Egypt", City = "Cairo" },
            new { Name = "Hana", Age = 22, Country = "Egypt", City = "Alexandria" },
            new { Name = "Layla", Age = 26, Country = "Lebanon", City = "Beirut" },
            new { Name = "Julia", Age = 25, Country = "Germany", City = "Berlin" },
            new { Name = "Jessica", Age = 23, Country = "USA", City = "New York" },
            new { Name = "Amira", Age = 27, Country = "UAE", City = "Dubai" },
            new { Name = "Nour", Age = 21, Country = "Egypt", City = "Giza" },
            new { Name = "Sophie", Age = 28, Country = "France", City = "Paris" },
            new { Name = "Emma", Age = 24, Country = "UK", City = "London" },
            new { Name = "Yara", Age = 25, Country = "Jordan", City = "Amman" }
        };

        foreach (var user in baseUsers)
        {
            // Filter Logic (Mock)
            if (request.Country != null && user.Country != request.Country) continue;
            if (request.MinAge.HasValue && user.Age < request.MinAge.Value) continue;
            if (request.MaxAge.HasValue && user.Age > request.MaxAge.Value) continue;

            double matchScore = rng.Next(60, 99); // Random match %
            bool isBoosted = rng.NextDouble() > 0.8; // 20% chance
            bool isNew = rng.NextDouble() > 0.7;
            bool isOnline = rng.NextDouble() > 0.5;

            // Category Logic
            if (request.Category == "New" && !isNew) matchScore -= 50; // Penalize rank
            if (request.Category == "Popular" && matchScore < 80) matchScore -= 50;
            if (request.Category == "Online" && !isOnline) matchScore -= 100;

            if (matchScore > 0)
            {
                recommendations.Add(new RecommendationDto
                {
                    UserId = Guid.NewGuid(),
                    DisplayName = user.Name,
                    Age = user.Age,
                    City = user.City,
                    Country = user.Country,
                    AvatarUrl = $"https://ui-avatars.com/api/?name={user.Name}&background=random", // Placeholder
                    MatchPercentage = matchScore,
                    IsBoosted = isBoosted,
                    IsNew = isNew,
                    IsOnline = isOnline,
                    IsVerified = rng.NextDouble() > 0.3
                });
            }
        }

        // Apply Boosting Sort: Boosted first, then Match %
        var sorted = recommendations
            .OrderByDescending(r => r.IsBoosted)
            .ThenByDescending(r => r.MatchPercentage)
            .ToList();

        return Task.FromResult(sorted);
    }
}
