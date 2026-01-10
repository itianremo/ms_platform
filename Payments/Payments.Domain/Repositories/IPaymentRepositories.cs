using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Payments.Domain.Entities;
using Shared.Kernel; // IRepository defined here

namespace Payments.Domain.Repositories
{
    public interface ISubscriptionRepository : IRepository<Subscription>
    {
        Task<Subscription?> GetActiveSubscriptionAsync(Guid userId, Guid appId);
        
        // Plans are managed here or in a separate repository? 
        // Let's keep Plan management separate or here for simplicity if needed.
        // But better to have IPlanRepository.
    }

    public interface IPlanRepository : IRepository<Plan>
    {
        Task<Plan?> GetByProviderIdAsync(string providerPlanId);
        Task<List<Plan>> GetByAppIdAsync(string appId); // AppId is string in my entity
    }

    public interface IPaymentTransactionRepository : IRepository<Transaction>
    {
        Task<List<Transaction>> GetByUserIdAsync(Guid userId);
    }

    public interface IAppPaymentProviderRepository : IRepository<AppPaymentProvider>
    {
        Task<List<AppPaymentProvider>> GetByAppIdAsync(string appId);
        Task<AppPaymentProvider?> GetAsync(string appId, string gatewayName);
    }
}
