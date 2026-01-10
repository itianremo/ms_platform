using Microsoft.AspNetCore.Mvc;
using Search.Domain.Entities;
using Search.Domain.Repositories;

namespace Search.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly ISearchRepository _repository;

    public SearchController(ISearchRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<List<UserSearchProfile>>> Search([FromQuery] Guid appId, [FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest("Query cannot be empty");
        }

        var results = await _repository.SearchAsync(appId, q);
        return Ok(results);
    }
}
