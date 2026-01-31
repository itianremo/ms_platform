using Apps.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Apps.Infrastructure.Persistence;

public class AppsDbContext : DbContext
{
    public AppsDbContext(DbContextOptions<AppsDbContext> options) : base(options)
    {
    }

    public DbSet<AppConfig> Apps { get; set; }
    public DbSet<SubscriptionPackage> SubscriptionPackages { get; set; }
    public DbSet<UserSubscription> UserSubscriptions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
