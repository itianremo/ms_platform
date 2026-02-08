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
    Task<PagedResult<AuditLog>> GetPagedListAsync(int page, int pageSize, Guid? appId, Guid? userId);
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
        var query = await _context.AuditLogs
            .Where(x => x.Timestamp >= startDate)
            .GroupBy(x => x.Timestamp.Date)
            .Select(g => new DailyActivityDto(g.Key, g.Count()))
            .ToListAsync();
            
        return query;
    }

    public async Task<PagedResult<AuditLog>> GetPagedListAsync(int page, int pageSize, Guid? appId, Guid? userId)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (userId.HasValue)
        {
            var userIdString = userId.Value.ToString();
            // User actions OR actions on user
            query = query.Where(x => 
                x.UserId == userId || 
                (x.EntityId == userIdString && (x.EntityName == "User" || x.EntityName == "UserProfile"))
            );
        }

        if (appId.HasValue)
        {
            query = query.Where(x => x.AppId == appId);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(x => x.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<AuditLog>(items, totalCount, page, pageSize);
    }
}
