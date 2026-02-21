using MediatR;
using Recommendation.Application.DTOs;

namespace Recommendation.Application.Queries.GetRecommendations;

public class GetRecommendationsQuery : IRequest<List<RecommendationDto>>
{
    public Guid UserId { get; set; }
    public string Category { get; set; } = "All"; // All, New, Popular, Online, NearYou
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    
    // Basic Filters
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public string? Country { get; set; }
    
    // GPS & Distance
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? DistanceKm { get; set; }
    
    // Dynamic Attributes
    public string? InterestedIn { get; set; }
    public string? Drinking { get; set; }
    public string? Smoking { get; set; }
    public string? Education { get; set; }
    public string? EyeColor { get; set; }
    public string? HairColor { get; set; }
    public string? Religion { get; set; }
    public string? Intent { get; set; }
    public string? Lifestyle { get; set; }
}
