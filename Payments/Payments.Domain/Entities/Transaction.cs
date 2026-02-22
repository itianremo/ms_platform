using System;
using Shared.Kernel;

namespace Payments.Domain.Entities
{
    public class Transaction : Entity
    {
        public Guid? SubscriptionId { get; set; }
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; } // Success, Failed, Pending
        public string ProviderTransactionId { get; set; }
        public string PaymentGateway { get; set; } // "Stripe", "PayTabs"
        public string GatewayResponse { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
