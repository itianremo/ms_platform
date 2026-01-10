using System;
using Shared.Kernel;

namespace Payments.Domain.Entities
{
    public class Plan : Entity
    {
        public string Name { get; set; }
        public string AppId { get; set; } // Tenant isolation
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public string Interval { get; set; } // "Monthly", "Yearly"
        public string ProviderPlanId { get; set; } // Stripe Price ID, PayTabs Plan ID, etc.
        public bool IsActive { get; set; } = true;
    }
}
