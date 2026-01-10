namespace Payments.Domain.Interfaces
{
    public class SubscriptionResult
    {
        public bool Success { get; set; }
        public string ProviderSubscriptionId { get; set; }
        public string Status { get; set; }
    }
}
