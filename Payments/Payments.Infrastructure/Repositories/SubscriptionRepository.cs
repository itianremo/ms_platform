using Microsoft.EntityFrameworkCore;
using Payments.Domain.Entities;
using Payments.Domain.Repositories;
using Payments.Infrastructure.Persistence;

namespace Payments.Infrastructure.Repositories
{
    public class SubscriptionRepository : Repository<Subscription>, ISubscriptionRepository
    {
        private readonly PaymentsDbContext _context;

        public SubscriptionRepository(PaymentsDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Subscription?> GetActiveSubscriptionAsync(Guid userId, Guid appId)
        {
            return await _context.Subscriptions
                .Include(x => x.Plan)
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Plan.AppId == appId.ToString() && x.Status == "Active");
        }
    }
}
