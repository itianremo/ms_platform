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
}
