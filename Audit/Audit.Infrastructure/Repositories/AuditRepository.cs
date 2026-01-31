using Audit.Domain.Entities;
using Audit.Infrastructure.Persistence;
using Shared.Kernel;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Audit.Infrastructure.Repositories;

public interface IAuditRepository : IRepository<AuditLog>
{
    // Custom query methods if needed
    Task<List<DailyActivityDto>> GetDailyStatsAsync(DateTime startDate);
}

public record DailyActivityDto(DateTime Date, int Count);

public class AuditRepository : IAuditRepository
{
    private readonly AuditDbContext _context;

    public AuditRepository(AuditDbContext context)
    {
        _context = context;
    }

    public async Task<AuditLog> AddAsync(AuditLog entity)
    {
        await _context.AuditLogs.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<AuditLog?> GetByIdAsync(Guid id)
    {
        return await _context.AuditLogs.FindAsync(id);
    }

    public async Task<List<AuditLog>> ListAsync()
    {
        return await _context.AuditLogs.ToListAsync();
    }

    public async Task<List<AuditLog>> ListAsync(Expression<Func<AuditLog, bool>> predicate)
    {
        return await _context.AuditLogs.Where(predicate).ToListAsync();
    }

    public async Task UpdateAsync(AuditLog entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(AuditLog entity)
    {
        _context.AuditLogs.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<List<DailyActivityDto>> GetDailyStatsAsync(DateTime startDate)
    {
        // Simple grouping by Date
        // Note: EF Core might need client evaluation if provider doesn't support Date truncation well
        // But for SQL Server/Postgres, usually works with .Date property or DbFunctions.
        
        var query = await _context.AuditLogs
            .Where(x => x.Timestamp >= startDate)
            .GroupBy(x => x.Timestamp.Date)
            .Select(g => new DailyActivityDto(g.Key, g.Count()))
            .ToListAsync();
            
        return query;
    }
}
