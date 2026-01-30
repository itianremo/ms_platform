using Apps.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Apps.API.Factories;

public class AppsDbContextFactory : IDesignTimeDbContextFactory<AppsDbContext>
{
    public AppsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppsDbContext>();
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=AppsDb;Trusted_Connection=True;MultipleActiveResultSets=true");

        return new AppsDbContext(optionsBuilder.Options);
    }
}
