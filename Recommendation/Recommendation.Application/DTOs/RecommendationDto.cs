namespace Recommendation.Application.DTOs;

public class RecommendationDto
{
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string AvatarUrl { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public List<string> Images { get; set; } = new();
    
    // Discovery Features
    public double MatchPercentage { get; set; } // 0-100
    public bool IsBoosted { get; set; }
    public bool IsNew { get; set; }
    public bool IsOnline { get; set; }
    public bool IsVerified { get; set; } // Photo Verified
}
