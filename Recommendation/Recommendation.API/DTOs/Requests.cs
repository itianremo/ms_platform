namespace Recommendation.API.DTOs;

public class CandidateRequest
{
    public Guid UserId { get; set; }
    public List<Guid> CandidateIds { get; set; } = new();
}

public class SwipeRequest
{
    public Guid UserId { get; set; }
    public Guid TargetId { get; set; }
    public string Action { get; set; } // "Like", "Pass"
}

public class SwipeResponse
{
    public bool IsMatch { get; set; }
    public int? MatchId { get; set; }
    public int RemainingLikes { get; set; }
}
