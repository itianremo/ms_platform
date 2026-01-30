using Auth.Domain.Entities;
using Auth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Shared.Kernel;
using System.Linq.Expressions;

namespace Auth.Infrastructure.Repositories;

public class UserOtpRepository : IRepository<UserOtp>
{
    private readonly AuthDbContext _context;

    public UserOtpRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<UserOtp?> GetByIdAsync(Guid id)
    {
        return await _context.UserOtps.FindAsync(id);
    }

    public async Task<List<UserOtp>> ListAsync()
    {
        return await _context.UserOtps.ToListAsync();
    }

    public async Task<List<UserOtp>> ListAsync(Expression<Func<UserOtp, bool>> predicate)
    {
        return await _context.UserOtps.Where(predicate).ToListAsync();
    }

    public async Task<UserOtp> AddAsync(UserOtp entity)
    {
        await _context.UserOtps.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(UserOtp entity)
    {
        _context.UserOtps.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(UserOtp entity)
    {
        _context.UserOtps.Remove(entity);
        await _context.SaveChangesAsync();
    }
}
