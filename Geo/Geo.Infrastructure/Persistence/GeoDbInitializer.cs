using Geo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Geo.Infrastructure.Persistence;

public class GeoDbInitializer
{
    private readonly GeoDbContext _context;
    private readonly ILogger<GeoDbInitializer> _logger;

    public GeoDbInitializer(GeoDbContext context, ILogger<GeoDbInitializer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            if (_context.Database.IsNpgsql())
            {
                await _context.Database.MigrateAsync();
            }

            await SeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }

    private async Task SeedAsync()
    {
        if (!await _context.Countries.AnyAsync())
        {
            var countries = new List<Country>
            {
                new Country { Name = "United States", Code = "US", PhoneCode = "+1" },
                new Country { Name = "United Kingdom", Code = "GB", PhoneCode = "+44" },
                new Country { Name = "Canada", Code = "CA", PhoneCode = "+1" },
                new Country { Name = "Australia", Code = "AU", PhoneCode = "+61" },
                new Country { Name = "Germany", Code = "DE", PhoneCode = "+49" },
                new Country { Name = "France", Code = "FR", PhoneCode = "+33" },
                new Country { Name = "Egypt", Code = "EG", PhoneCode = "+20" },
                new Country { Name = "United Arab Emirates", Code = "AE", PhoneCode = "+971" },
                new Country { Name = "Saudi Arabia", Code = "SA", PhoneCode = "+966" },
            };

            await _context.Countries.AddRangeAsync(countries);
            await _context.SaveChangesAsync();

            // Seed Cities
            var us = countries.First(c => c.Code == "US");
            var uk = countries.First(c => c.Code == "GB");
            var eg = countries.First(c => c.Code == "EG");
            var ae = countries.First(c => c.Code == "AE");

            var cities = new List<City>
            {
                new City { Name = "New York", CountryId = us.Id },
                new City { Name = "Los Angeles", CountryId = us.Id },
                new City { Name = "Chicago", CountryId = us.Id },
                new City { Name = "London", CountryId = uk.Id },
                new City { Name = "Manchester", CountryId = uk.Id },
                
                // Egypt Cities
                new City { Name = "Cairo", CountryId = eg.Id },
                new City { Name = "Alexandria", CountryId = eg.Id },
                new City { Name = "Giza", CountryId = eg.Id },
                new City { Name = "Mersa Matruh", CountryId = eg.Id },
                new City { Name = "Port Said", CountryId = eg.Id },
                new City { Name = "Suez", CountryId = eg.Id },
                new City { Name = "Luxor", CountryId = eg.Id },
                new City { Name = "Aswan", CountryId = eg.Id },
                new City { Name = "Hurghada", CountryId = eg.Id },
                new City { Name = "Sharm El Sheikh", CountryId = eg.Id },
                new City { Name = "Dahab", CountryId = eg.Id },
                new City { Name = "Tanta", CountryId = eg.Id },
                new City { Name = "Mansoura", CountryId = eg.Id },
                new City { Name = "Fayoum", CountryId = eg.Id },
                new City { Name = "Zagazig", CountryId = eg.Id },
                new City { Name = "Ismailia", CountryId = eg.Id },
                new City { Name = "Minya", CountryId = eg.Id },
                new City { Name = "Asyut", CountryId = eg.Id },
                new City { Name = "Sohag", CountryId = eg.Id },
                new City { Name = "Qena", CountryId = eg.Id },
                new City { Name = "Beni Suef", CountryId = eg.Id },
                
                new City { Name = "Dubai", CountryId = ae.Id },
                new City { Name = "Abu Dhabi", CountryId = ae.Id },
            };

            await _context.Cities.AddRangeAsync(cities);
            await _context.SaveChangesAsync();
        }
    }
}
