using System;
using MediatR;

namespace Payments.Application.Features.Payments.Commands.SubscribeUser
{
    public class SubscribeUserCommand : IRequest<SubscribeUserResponse>
    {
        public Guid UserId { get; set; }
        public Guid PlanId { get; set; }
        public string Email { get; set; }
        public string PaymentProvider { get; set; } // "Stripe", "PayTabs", "Mock"
    }

    public class SubscribeUserResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Guid SubscriptionId { get; set; }
        public string ProviderSubscriptionId { get; set; }
        public string RedirectUrl { get; set; }
    }
}
