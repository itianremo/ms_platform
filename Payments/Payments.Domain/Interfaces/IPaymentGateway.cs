using System;
using System.Threading.Tasks;
using Payments.Domain.Entities;

namespace Payments.Domain.Interfaces
{
    public interface IPaymentGateway
    {
        Task<string> InitiatePaymentAsync(decimal amount, string currency, string returnUrl);
        Task<SubscriptionResult> CreateSubscriptionAsync(string providerPlanId, string customerEmail);
        Task<bool> VerifyPaymentAsync(string transactionId);
    }

    public interface IPaymentGatewayFactory
    {
        IPaymentGateway GetGateway(string gatewayName);
    }
}
