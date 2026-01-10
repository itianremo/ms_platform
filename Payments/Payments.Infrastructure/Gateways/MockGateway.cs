using System;
using System.Threading.Tasks;
using Payments.Domain.Interfaces;

namespace Payments.Infrastructure.Gateways
{
    public class MockGateway : IPaymentGateway
    {
        public Task<string> InitiatePaymentAsync(decimal amount, string currency, string returnUrl)
        {
            return Task.FromResult($"https://mock-gateway.com/pay?amount={amount}&currency={currency}");
        }

        public Task<SubscriptionResult> CreateSubscriptionAsync(string providerPlanId, string customerEmail)
        {
            return Task.FromResult(new SubscriptionResult
            {
                Success = true,
                ProviderSubscriptionId = $"sub_mock_{Guid.NewGuid()}",
                Status = "Active"
            });
        }

        public Task<bool> VerifyPaymentAsync(string transactionId)
        {
            return Task.FromResult(true);
        }
    }
}
