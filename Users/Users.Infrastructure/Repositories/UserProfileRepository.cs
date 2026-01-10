using Microsoft.EntityFrameworkCore;
using Shared.Kernel;
using System.Linq.Expressions;
using Users.Domain.Entities;
using Users.Domain.Repositories;
using Users.Infrastructure.Persistence;

namespace Users.Infrastructure.Repositories;

public class UserProfileRepository : IUserProfileRepository
{
    private readonly UsersDbContext _context;

    public UserProfileRepository(UsersDbContext context)
    {
        _context = context;
    }

    public async Task<UserProfile?> GetByIdAsync(Guid id)
    {
        return await _context.UserProfiles.FindAsync(id);
    }

    public async Task<UserProfile?> GetByUserIdAndAppIdAsync(Guid userId, Guid appId)
    {
        return await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId && p.AppId == appId);
    }

    public async Task<List<UserProfile>> ListAsync()
    {
        return await _context.UserProfiles.ToListAsync();
    }

    public async Task<List<UserProfile>> ListAsync(Expression<Func<UserProfile, bool>> predicate)
    {
        return await _context.UserProfiles.Where(predicate).ToListAsync();
    }

    public async Task<UserProfile> AddAsync(UserProfile entity)
    {
        await _context.UserProfiles.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(UserProfile entity)
    {
        _context.UserProfiles.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(UserProfile entity)
    {
        _context.UserProfiles.Remove(entity);
        await _context.SaveChangesAsync();
    }
}
