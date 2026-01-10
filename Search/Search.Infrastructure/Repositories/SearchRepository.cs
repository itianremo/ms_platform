using Microsoft.EntityFrameworkCore;
using Search.Domain.Entities;
using Search.Domain.Repositories;
using Search.Infrastructure.Persistence;
using Shared.Kernel;
using System.Linq.Expressions;

namespace Search.Infrastructure.Repositories;

public class SearchRepository : ISearchRepository
{
    private readonly SearchDbContext _context;

    public SearchRepository(SearchDbContext context)
    {
        _context = context;
    }

    public async Task<UserSearchProfile?> GetByIdAsync(Guid id)
    {
        return await _context.UserProfiles.FindAsync(id);
    }

    public async Task<List<UserSearchProfile>> ListAsync()
    {
        return await _context.UserProfiles.ToListAsync();
    }

    public async Task<List<UserSearchProfile>> ListAsync(Expression<Func<UserSearchProfile, bool>> predicate)
    {
        return await _context.UserProfiles.Where(predicate).ToListAsync();
    }

    public async Task<UserSearchProfile> AddAsync(UserSearchProfile entity)
    {
        await _context.UserProfiles.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(UserSearchProfile entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(UserSearchProfile entity)
    {
        _context.UserProfiles.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<List<UserSearchProfile>> SearchAsync(Guid appId, string query, int limit = 20)
    {
        // Simple "Contains" search for now. Real FTS with Postgres comes later.
        return await _context.UserProfiles
            .Where(x => x.AppId == appId && x.IsVisible && 
                   (x.DisplayName.Contains(query) || x.Bio.Contains(query)))
            .Take(limit)
            .ToListAsync();
    }
}
