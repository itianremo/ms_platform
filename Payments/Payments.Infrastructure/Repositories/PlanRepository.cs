using Microsoft.EntityFrameworkCore;
using Payments.Domain.Entities;
using Payments.Domain.Repositories;
using Payments.Infrastructure.Persistence;

namespace Payments.Infrastructure.Repositories
{
    public class PlanRepository : Repository<Plan>, IPlanRepository
    {
        private readonly PaymentsDbContext _context;

        public PlanRepository(PaymentsDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Plan?> GetByProviderIdAsync(string providerPlanId)
        {
            return await _context.Plans.FirstOrDefaultAsync(p => p.ProviderPlanId == providerPlanId);
        }

        public async Task<List<Plan>> GetByAppIdAsync(string appId)
        {
            return await _context.Plans.Where(p => p.AppId == appId).ToListAsync();
        }
    }
}
