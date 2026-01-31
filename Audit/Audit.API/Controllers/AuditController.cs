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
            if (appId.HasValue)
            {
                return await _repository.ListAsync(x => x.AppId == appId && x.UserId == userId);
            }
            return await _repository.ListAsync(x => x.UserId == userId);
        }
        else if (appId.HasValue)
        {
            return await _repository.ListAsync(x => x.AppId == appId);
        }
        
        return await _repository.ListAsync(); // Should be paged in real app
    }

    [HttpGet("stats")]
    public async Task<ActionResult<List<DailyActivityDto>>> GetStats([FromQuery] int days = 7)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);
        var stats = await _repository.GetDailyStatsAsync(startDate);
        return Ok(stats);
    }
}
