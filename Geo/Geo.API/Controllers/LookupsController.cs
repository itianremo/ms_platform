using Geo.Domain.Entities;
using Geo.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
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

    [Authorize(Policy = "ManageLookups")]
    [HttpPost("countries")]
    public async Task<ActionResult<Country>> CreateCountry(CreateCountryDto dto)
    {
        var country = new Country { Name = dto.Name, Code = dto.Code, PhoneCode = dto.PhoneCode };
        _context.Countries.Add(country);
        await _context.SaveChangesAsync();
        return Ok(country);
    }

    [Authorize(Policy = "ManageLookups")]
    [HttpPut("countries/{id}")]
    public async Task<ActionResult<Country>> UpdateCountry(Guid id, UpdateCountryDto dto)
    {
        var country = await _context.Countries.FindAsync(id);
        if (country == null) return NotFound();

        country.Name = dto.Name;
        country.Code = dto.Code;
        country.PhoneCode = dto.PhoneCode;
        country.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        return Ok(country);
    }

    [Authorize(Policy = "ManageLookups")]
    [HttpDelete("countries/{id}")]
    public async Task<IActionResult> DeleteCountry(Guid id)
    {
        var country = await _context.Countries.FindAsync(id);
        if (country == null) return NotFound();

        country.IsActive = false; // Soft Delete
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("cities/{countryId}")]
    public async Task<ActionResult<List<City>>> GetCities(string countryId)
    {
        if (!Guid.TryParse(countryId, out var countryGuid))
            return BadRequest("Invalid country ID");

        return await _context.Cities
            .Where(c => c.CountryId == countryGuid && c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    [Authorize(Policy = "ManageLookups")]
    [HttpPost("cities")]
    public async Task<ActionResult<City>> CreateCity(CreateCityDto dto)
    {
        var city = new City { Name = dto.Name, CountryId = dto.CountryId };
        _context.Cities.Add(city);
        await _context.SaveChangesAsync();
        return Ok(city);
    }

    [Authorize(Policy = "ManageLookups")]
    [HttpPut("cities/{id}")]
    public async Task<ActionResult<City>> UpdateCity(Guid id, UpdateCityDto dto)
    {
        var city = await _context.Cities.FindAsync(id);
        if (city == null) return NotFound();

        city.Name = dto.Name;
        city.CountryId = dto.CountryId;
        city.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        return Ok(city);
    }

    [Authorize(Policy = "ManageLookups")]
    [HttpDelete("cities/{id}")]
    public async Task<IActionResult> DeleteCity(Guid id)
    {
        var city = await _context.Cities.FindAsync(id);
        if (city == null) return NotFound();

        city.IsActive = false; // Soft Delete
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

public class CreateCountryDto { public string Name { get; set; } = ""; public string Code { get; set; } = ""; public string PhoneCode { get; set; } = ""; }
public class UpdateCountryDto { public string Name { get; set; } = ""; public string Code { get; set; } = ""; public string PhoneCode { get; set; } = ""; public bool IsActive { get; set; } }
public class CreateCityDto { public string Name { get; set; } = ""; public Guid CountryId { get; set; } }
public class UpdateCityDto { public string Name { get; set; } = ""; public Guid CountryId { get; set; } public bool IsActive { get; set; } }
