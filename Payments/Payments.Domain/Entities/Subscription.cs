using System;
using Shared.Kernel;

namespace Payments.Domain.Entities
{
    public class Subscription : Entity
    {
        public Guid UserId { get; set; }
        public Guid PlanId { get; set; }
        public Plan Plan { get; set; }
        public string Status { get; set; } // Active, Cancelled, PastDue
        public string ProviderSubscriptionId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? NextBillingDate { get; set; }
        public string PaymentGateway { get; set; } // "Stripe", "PayTabs"
    }
}
