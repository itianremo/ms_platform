using Apps.Domain.Entities;
using Apps.Domain.Repositories;
using Apps.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Apps.Infrastructure.Repositories;

public class UserSubscriptionRepository : IUserSubscriptionRepository
{
    private readonly AppsDbContext _context;

    public UserSubscriptionRepository(AppsDbContext context)
    {
        _context = context;
    }

    public async Task<UserSubscription?> GetByIdAsync(Guid id)
    {
        return await _context.UserSubscriptions.FindAsync(id);
    }

    public async Task<List<UserSubscription>> GetByUserIdAsync(Guid appId, Guid userId)
    {
        return await _context.UserSubscriptions
            .Where(s => s.AppId == appId && s.UserId == userId)
            .OrderByDescending(s => s.StartDate)
            .ToListAsync();
    }

    public async Task AddAsync(UserSubscription subscription)
    {
        await _context.UserSubscriptions.AddAsync(subscription);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(UserSubscription subscription)
    {
        _context.UserSubscriptions.Update(subscription);
        await _context.SaveChangesAsync();
    }
}
