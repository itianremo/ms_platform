using Microsoft.AspNetCore.Mvc;
using Recommendation.Domain.Entities;

namespace Recommendation.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecommendationController : ControllerBase
{
    private readonly IRecommendationEngine _engine;

    public RecommendationController(IRecommendationEngine engine)
    {
        _engine = engine;
    }

    [HttpGet("predict")]
    public ActionResult<float> Predict([FromQuery] int userId, [FromQuery] int itemId)
    {
        var score = _engine.Predict(userId, itemId);
        return Ok(score);
    }
    
    // In real app, we would have POST /train to retrain model 
    // or a scheduled job to retrain periodically
}
