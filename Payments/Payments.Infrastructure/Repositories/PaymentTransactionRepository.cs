using Microsoft.EntityFrameworkCore;
using Payments.Domain.Entities;
using Payments.Domain.Repositories;
using Payments.Infrastructure.Persistence;

namespace Payments.Infrastructure.Repositories
{
    public class PaymentTransactionRepository : Repository<Transaction>, IPaymentTransactionRepository
    {
        private readonly PaymentsDbContext _context;

        public PaymentTransactionRepository(PaymentsDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Transaction>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Transactions
                .Where(x => x.UserId == userId)
                .ToListAsync();
        }
    }
}
