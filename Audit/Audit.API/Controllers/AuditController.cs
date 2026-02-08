using Audit.Domain.Entities;
using Audit.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Shared.Kernel;

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
    public async Task<ActionResult<PagedResult<AuditLog>>> GetLogs(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 25,
        [FromQuery] Guid? appId = null, 
        [FromQuery] Guid? userId = null)
    {
        // Ensure valid page/pageSize
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var result = await _repository.GetPagedListAsync(page, pageSize, appId, userId);
        return Ok(result);
    }

    [HttpGet("stats")]
    public async Task<ActionResult<List<DailyActivityDto>>> GetStats([FromQuery] int days = 7)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);
        var stats = await _repository.GetDailyStatsAsync(startDate);
        return Ok(stats);
    }
}
