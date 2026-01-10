using System;
using Microsoft.Extensions.DependencyInjection;
using Payments.Domain.Interfaces;

namespace Payments.Infrastructure.Gateways
{
    public class PaymentGatewayFactory : IPaymentGatewayFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public PaymentGatewayFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IPaymentGateway GetGateway(string gatewayName)
        {
            return gatewayName.ToLower() switch
            {
                "stripe" => _serviceProvider.GetRequiredService<StripeGateway>(),
                "paytabs" => throw new NotImplementedException("PayTabs not yet implemented"),
                "mock" => _serviceProvider.GetRequiredService<MockGateway>(),
                _ => _serviceProvider.GetRequiredService<MockGateway>() // Default to Mock
            };
        }
    }
}
