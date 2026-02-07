namespace Auth.Application.Features.Auth.Queries.GetAppUserStats;

public class AppUserStatsDto
{
    public Guid AppId { get; set; }
    public string AppName { get; set; } // Optional, might need to fetch from Apps service or just return ID
    public int AdminCount { get; set; }
    public int VisitorCount { get; set; }
}
