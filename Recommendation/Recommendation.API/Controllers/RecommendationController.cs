using Microsoft.AspNetCore.Mvc;
using Recommendation.API.DTOs;
using Recommendation.Domain.Entities;
using Recommendation.Application.Common.Interfaces;

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
    public ActionResult<float> Predict([FromQuery] Guid userId, [FromQuery] Guid itemId)
    {
        // Mock Engine uses int, hashing Guid for now
        var score = _engine.Predict(userId.GetHashCode(), itemId.GetHashCode());
        return Ok(score);
    }
    
    [HttpPost("candidates")]
    public ActionResult<List<Guid>> GetCandidates([FromBody] CandidateRequest request)
    {
        var swiped = _store.GetSwipedUsers(request.UserId);
        
        // Filter out already swiped users
        var validCandidates = request.CandidateIds.Except(swiped).ToList();

        // Score them
        var scored = validCandidates.Select(id => new 
        { 
            Id = id, 
            Score = _engine.Predict(request.UserId.GetHashCode(), id.GetHashCode()) 
        }).OrderByDescending(x => x.Score).ToList();

        return Ok(scored.Select(x => x.Id).ToList());
    }

    [HttpPost("swipe")]
    public ActionResult<SwipeResponse> Swipe([FromBody] SwipeRequest request)
    {
         // 1. Check Limits
        var stats = _store.GetSwipeInfo(request.UserId);
        if (request.Action == "Like" && stats.Remaining <= 0)
        {
             return BadRequest("Likes are out of credit");
        }

        // 2. Add Swipe
        _store.AddSwipe(request.UserId, request.TargetId, request.Action);

        // 3. Get Updated Stats
        stats = _store.GetSwipeInfo(request.UserId);

        var response = new SwipeResponse 
        { 
            IsMatch = false,
            RemainingLikes = stats.Remaining
        };

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

    [HttpGet("stats")]
    public ActionResult<object> GetStats([FromQuery] Guid userId)
    {
        var stats = _store.GetSwipeInfo(userId);
        return Ok(new { 
            Count = stats.Count,
            FirstLikeTime = stats.FirstLikeTime,
            RemainingLikes = stats.Remaining
        });
    }
}
