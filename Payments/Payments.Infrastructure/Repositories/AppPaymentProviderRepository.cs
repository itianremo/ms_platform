using Microsoft.EntityFrameworkCore;
using Payments.Domain.Entities;
using Payments.Domain.Repositories;
using Payments.Infrastructure.Persistence;

namespace Payments.Infrastructure.Repositories
{
    public class AppPaymentProviderRepository : Repository<AppPaymentProvider>, IAppPaymentProviderRepository
    {
        private readonly PaymentsDbContext _context;

        public AppPaymentProviderRepository(PaymentsDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<AppPaymentProvider>> GetByAppIdAsync(string appId)
        {
            return await _context.AppPaymentProviders
                .Where(x => x.AppId == appId && x.IsEnabled)
                .ToListAsync();
        }

        public async Task<AppPaymentProvider?> GetAsync(string appId, string gatewayName)
        {
            return await _context.AppPaymentProviders
                .FirstOrDefaultAsync(x => x.AppId == appId && x.GatewayName == gatewayName);
        }
    }
}
