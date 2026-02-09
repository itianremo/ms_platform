using Geo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Geo.Infrastructure.Persistence;

public class GeoDbContext : DbContext
{
    public GeoDbContext(DbContextOptions<GeoDbContext> options) : base(options) { }

    public DbSet<GeoLocation> Locations { get; set; }
    public DbSet<Country> Countries { get; set; }
    public DbSet<City> Cities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasPostgresExtension("postgis");

        modelBuilder.Entity<GeoLocation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.AppId, e.UserId }).IsUnique();
            entity.HasIndex(e => e.Location).HasMethod("gist");
        });
    }
}
