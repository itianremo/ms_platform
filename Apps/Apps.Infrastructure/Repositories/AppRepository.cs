using Apps.Domain.Entities;
using Apps.Domain.Repositories;
using Apps.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Shared.Kernel;
using System.Linq.Expressions;

namespace Apps.Infrastructure.Repositories;

public class AppRepository : IAppRepository
{
    private readonly AppsDbContext _context;

    public AppRepository(AppsDbContext context)
    {
        _context = context;
    }

    public async Task<AppConfig?> GetByIdAsync(Guid id)
    {
        return await _context.Apps.FindAsync(id);
    }

    public async Task<AppConfig?> GetByNameAsync(string name)
    {
        return await _context.Apps.FirstOrDefaultAsync(a => a.Name == name);
    }

    public async Task<List<AppConfig>> ListAsync()
    {
        return await _context.Apps.ToListAsync();
    }

    public async Task<List<AppConfig>> ListAsync(Expression<Func<AppConfig, bool>> predicate)
    {
        return await _context.Apps.Where(predicate).ToListAsync();
    }

    public async Task<AppConfig> AddAsync(AppConfig entity)
    {
        await _context.Apps.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(AppConfig entity)
    {
        _context.Apps.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(AppConfig entity)
    {
        _context.Apps.Remove(entity);
        await _context.SaveChangesAsync();
    }
}
