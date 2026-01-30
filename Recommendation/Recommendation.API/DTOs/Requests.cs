namespace Recommendation.API.DTOs;

public class CandidateRequest
{
    public int UserId { get; set; }
    public List<int> CandidateIds { get; set; } = new();
}

public class SwipeRequest
{
    public int UserId { get; set; }
    public int TargetId { get; set; }
    public string Action { get; set; } // "Like", "Pass"
}

public class SwipeResponse
{
    public bool IsMatch { get; set; }
    public int? MatchId { get; set; }
}
