using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recommendation.Application.DTOs;
using Recommendation.Application.Queries.GetRecommendations;

namespace Recommendation.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecommendationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RecommendationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<RecommendationDto>>> GetRecommendations(
        [FromQuery] string category = "All",
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20,
        [FromQuery] int? minAge = null,
        [FromQuery] int? maxAge = null,
        [FromQuery] string? country = null)
    {
        // TODO: Get UserId from Claims
        var userId = Guid.Empty; 

        var query = new GetRecommendationsQuery
        {
            UserId = userId,
            Category = category,
            Page = page,
            PageSize = pageSize,
            MinAge = minAge,
            MaxAge = maxAge,
            Country = country
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
