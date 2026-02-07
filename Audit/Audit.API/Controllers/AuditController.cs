using Audit.Domain.Entities;
using Audit.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Audit.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditController : ControllerBase
{
    private readonly IAuditRepository _repository;

    public AuditController(IAuditRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuditLog>>> GetLogs([FromQuery] Guid? appId, [FromQuery] Guid? userId)
    {
        if (userId.HasValue)
        {
            var userIdString = userId.Value.ToString();
            
            // Search for:
            // 1. Actions performed BY the user (UserId == userId)
            // 2. Actions performed ON the user (EntityId == userIdString AND EntityName is "User" or "UserProfile")
            
            if (appId.HasValue)
            {
                return await _repository.ListAsync(x => 
                    x.AppId == appId && 
                    (x.UserId == userId || (x.EntityId == userIdString && (x.EntityName == "User" || x.EntityName == "UserProfile")))
                );
            }
            
            return await _repository.ListAsync(x => 
                x.UserId == userId || 
                (x.EntityId == userIdString && (x.EntityName == "User" || x.EntityName == "UserProfile"))
            );
        }
        else if (appId.HasValue)
        {
            return await _repository.ListAsync(x => x.AppId == appId);
        }
        
        // Return recent logs (limit 100 for safety)
        var allLogs = await _repository.ListAsync();
        return allLogs.OrderByDescending(x => x.Timestamp).Take(100).ToList();
    }

    [HttpGet("stats")]
    public async Task<ActionResult<List<DailyActivityDto>>> GetStats([FromQuery] int days = 7)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);
        var stats = await _repository.GetDailyStatsAsync(startDate);
        return Ok(stats);
    }
}
