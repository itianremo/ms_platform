using Geo.Domain.Entities;
using Geo.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Geo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LookupsController : ControllerBase
{
    private readonly GeoDbContext _context;

    public LookupsController(GeoDbContext context)
    {
        _context = context;
    }

    [HttpGet("countries")]
    public async Task<ActionResult<List<Country>>> GetCountries()
    {
        return await _context.Countries
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    [HttpGet("cities/{countryId}")]
    public async Task<ActionResult<List<City>>> GetCities(string countryId)
    {
        if (!Guid.TryParse(countryId, out var countryGuid))
        {
            return BadRequest("Invalid country ID");
        }

        return await _context.Cities
            .Where(c => c.CountryId == countryGuid && c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }
}
