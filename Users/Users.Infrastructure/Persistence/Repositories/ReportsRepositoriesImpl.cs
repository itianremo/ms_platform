using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Users.Domain.Entities;
using Users.Domain.Repositories;
using Users.Infrastructure.Persistence;

namespace Users.Infrastructure.Persistence.Repositories;

public class ReportReasonRepository : IReportReasonRepository
{
    private readonly UsersDbContext _dbContext;

    public ReportReasonRepository(UsersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ReportReason?> GetByIdAsync(Guid id) => await _dbContext.ReportReasons.FindAsync(id);

    public async Task<List<ReportReason>> ListAsync() => await _dbContext.ReportReasons.ToListAsync();

    public async Task<List<ReportReason>> ListAsync(Expression<Func<ReportReason, bool>> predicate) => await _dbContext.ReportReasons.Where(predicate).ToListAsync();

    public async Task<ReportReason> AddAsync(ReportReason entity)
    {
        _dbContext.ReportReasons.Add(entity);
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(ReportReason entity)
    {
        _dbContext.Entry(entity).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(ReportReason entity)
    {
        _dbContext.ReportReasons.Remove(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<ReportReason>> GetActiveReasonsAsync(Guid appId)
    {
        return await _dbContext.ReportReasons
            .Where(r => r.AppId == appId && r.IsActive)
            .ToListAsync();
    }
}

public class UserReportRepository : IUserReportRepository
{
    private readonly UsersDbContext _dbContext;

    public UserReportRepository(UsersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserReport?> GetByIdAsync(Guid id) => await _dbContext.UserReports.FindAsync(id);

    public async Task<List<UserReport>> ListAsync() => await _dbContext.UserReports.ToListAsync();

    public async Task<List<UserReport>> ListAsync(Expression<Func<UserReport, bool>> predicate) => await _dbContext.UserReports.Where(predicate).ToListAsync();

    public async Task<UserReport> AddAsync(UserReport entity)
    {
        _dbContext.UserReports.Add(entity);
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(UserReport entity)
    {
        _dbContext.Entry(entity).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(UserReport entity)
    {
        _dbContext.UserReports.Remove(entity);
        await _dbContext.SaveChangesAsync();
    }
}
