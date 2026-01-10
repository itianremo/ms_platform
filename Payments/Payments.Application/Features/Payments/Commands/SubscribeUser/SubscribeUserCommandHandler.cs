using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Payments.Domain.Entities;
using Payments.Domain.Repositories;
using Payments.Domain.Interfaces;

namespace Payments.Application.Features.Payments.Commands.SubscribeUser
{
    public class SubscribeUserCommandHandler : IRequestHandler<SubscribeUserCommand, SubscribeUserResponse>
    {
        private readonly IPaymentGatewayFactory _gatewayFactory;
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly IPlanRepository _planRepository;
        private readonly IAppPaymentProviderRepository _configRepository;

        public SubscribeUserCommandHandler(
            IPaymentGatewayFactory gatewayFactory, 
            ISubscriptionRepository subscriptionRepository,
            IPlanRepository planRepository,
            IAppPaymentProviderRepository configRepository)
        {
            _gatewayFactory = gatewayFactory;
            _subscriptionRepository = subscriptionRepository;
            _planRepository = planRepository;
            _configRepository = configRepository;
        }

        public async Task<SubscribeUserResponse> Handle(SubscribeUserCommand request, CancellationToken cancellationToken)
        {
            // 0. Validate Plan & Config
            var plan = await _planRepository.GetByIdAsync(request.PlanId);
            if (plan == null) return new SubscribeUserResponse { Success = false, Message = "Plan not found." };

            var config = await _configRepository.GetAsync(plan.AppId, request.PaymentProvider);
            if (config == null || !config.IsEnabled)
            {
                return new SubscribeUserResponse { Success = false, Message = $"Payment provider '{request.PaymentProvider}' is not available for this app." };
            }

            // 1. Get Strategy
            var gateway = _gatewayFactory.GetGateway(request.PaymentProvider);

            // 2. Create Subscription on Provider
            var result = await gateway.CreateSubscriptionAsync(plan.ProviderPlanId, request.Email);

            if (!result.Success)
            {
                return new SubscribeUserResponse { Success = false, Message = "Gateway failed to create subscription." };
            }

            // 3. Save to DB
            var subscription = new Subscription
            {
                UserId = request.UserId,
                PlanId = request.PlanId,
                Status = result.Status,
                ProviderSubscriptionId = result.ProviderSubscriptionId,
                StartDate = DateTime.UtcNow,
                PaymentGateway = request.PaymentProvider
            };

            await _subscriptionRepository.AddAsync(subscription);

            return new SubscribeUserResponse
            {
                Success = true,
                SubscriptionId = subscription.Id,
                ProviderSubscriptionId = result.ProviderSubscriptionId,
                Message = "Subscription created successfully."
            };
        }
    }
}
