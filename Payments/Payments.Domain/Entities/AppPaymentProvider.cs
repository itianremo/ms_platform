using Shared.Kernel;

namespace Payments.Domain.Entities
{
    public class AppPaymentProvider : Entity
    {
        public string AppId { get; set; } // "FitIT", "Wissler"
        public string GatewayName { get; set; } // "Stripe", "PayTabs", "Mock"
        public bool IsEnabled { get; set; }
        public string ConfigJson { get; set; } // JSON string for API Keys/Secrets

        public AppPaymentProvider() { }

        public AppPaymentProvider(Guid id, string appId, string gatewayName, bool isEnabled, string configJson)
        {
            Id = id;
            AppId = appId;
            GatewayName = gatewayName;
            IsEnabled = isEnabled;
            ConfigJson = configJson;
        }
    }
}
