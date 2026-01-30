namespace Payments.Application.Features.Payments.Queries.GetAppPaymentMethods
{
    public class PaymentMethodDto
    {
        public string GatewayName { get; set; }
        public bool IsEnabled { get; set; }
        public string ConfigJson { get; set; }
    }
}
