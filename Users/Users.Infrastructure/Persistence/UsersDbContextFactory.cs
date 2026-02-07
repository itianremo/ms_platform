using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Users.Infrastructure.Persistence;

public class UsersDbContextFactory : IDesignTimeDbContextFactory<UsersDbContext>
{
    public UsersDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<UsersDbContext>();
        
        // Use a dummy or development connection string for migration generation.
        // It does not need to be valid for "add migration" unless database inspection is required (rare for code-first).
        // Using the standard dev string just in case.
        optionsBuilder.UseSqlServer("Server=localhost,14333;Database=UsersDb;User Id=sa;Password=Password123!;TrustServerCertificate=True;");

        return new UsersDbContext(optionsBuilder.Options);
    }
}
