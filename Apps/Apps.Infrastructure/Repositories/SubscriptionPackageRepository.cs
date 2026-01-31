using Apps.Domain.Entities;
using Apps.Domain.Repositories;
using Apps.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Apps.Infrastructure.Repositories;

public class SubscriptionPackageRepository : ISubscriptionPackageRepository
{
    private readonly AppsDbContext _context;

    public SubscriptionPackageRepository(AppsDbContext context)
    {
        _context = context;
    }

    public async Task<SubscriptionPackage?> GetByIdAsync(Guid id)
    {
        return await _context.SubscriptionPackages.FindAsync(id);
    }

    public async Task<List<SubscriptionPackage>> ListAsync()
    {
        return await _context.SubscriptionPackages.ToListAsync();
    }

    public async Task<List<SubscriptionPackage>> ListAsync(System.Linq.Expressions.Expression<Func<SubscriptionPackage, bool>> predicate)
    {
        return await _context.SubscriptionPackages.Where(predicate).ToListAsync();
    }

    public async Task<SubscriptionPackage> AddAsync(SubscriptionPackage entity)
    {
        await _context.SubscriptionPackages.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(SubscriptionPackage entity)
    {
        _context.SubscriptionPackages.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(SubscriptionPackage entity)
    {
        _context.SubscriptionPackages.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<List<SubscriptionPackage>> GetByAppIdAsync(Guid appId)
    {
        return await _context.SubscriptionPackages
            .Where(p => p.AppId == appId && p.IsActive)
            .ToListAsync();
    }
}
