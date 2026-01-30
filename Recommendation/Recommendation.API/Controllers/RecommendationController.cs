using Microsoft.AspNetCore.Mvc;
using Recommendation.API.DTOs;
using Recommendation.Domain.Entities;
using Recommendation.Infrastructure.Services;

namespace Recommendation.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecommendationController : ControllerBase
{
    private readonly IRecommendationEngine _engine;
    private readonly IRecommendationStore _store;

    public RecommendationController(IRecommendationEngine engine, IRecommendationStore store)
    {
        _engine = engine;
        _store = store;
    }

    [HttpGet("predict")]
    public ActionResult<float> Predict([FromQuery] int userId, [FromQuery] int itemId)
    {
        var score = _engine.Predict(userId, itemId);
        return Ok(score);
    }
    
    [HttpPost("candidates")]
    public ActionResult<List<int>> GetCandidates([FromBody] CandidateRequest request)
    {
        var swiped = _store.GetSwipedUsers(request.UserId);
        
        // Filter out already swiped users
        var validCandidates = request.CandidateIds.Except(swiped).ToList();

        // Score them
        var scored = validCandidates.Select(id => new 
        { 
            Id = id, 
            Score = _engine.Predict(request.UserId, id) 
        }).OrderByDescending(x => x.Score).ToList();

        return Ok(scored.Select(x => x.Id).ToList());
    }

    [HttpPost("swipe")]
    public ActionResult<SwipeResponse> Swipe([FromBody] SwipeRequest request)
    {
        _store.AddSwipe(request.UserId, request.TargetId, request.Action);

        var response = new SwipeResponse { IsMatch = false };

        if (request.Action == "Like")
        {
            if (_store.IsMatch(request.UserId, request.TargetId))
            {
                response.IsMatch = true;
                // In real app, we would create a Match record in DB here and return its ID
                response.MatchId = new Random().Next(1000, 9999); 
            }
        }

        return Ok(response);
    }
}
