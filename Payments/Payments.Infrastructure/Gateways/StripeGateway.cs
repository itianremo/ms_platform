using Microsoft.Extensions.Configuration;
using Payments.Domain.Interfaces;
using Stripe;
using Stripe.Checkout;

namespace Payments.Infrastructure.Gateways;

public class StripeGateway : IPaymentGateway
{
    private readonly string _apiKey;

    public StripeGateway(IConfiguration configuration)
    {
        _apiKey = configuration["Stripe:SecretKey"];
        StripeConfiguration.ApiKey = _apiKey;
    }

    public async Task<string> InitiatePaymentAsync(decimal amount, string currency, string returnUrl)
    {
        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(amount * 100), // Stripe expects cents
                        Currency = currency,
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "One-time Payment",
                        },
                    },
                    Quantity = 1,
                },
            },
            Mode = "payment",
            SuccessUrl = returnUrl + "?session_id={CHECKOUT_SESSION_ID}",
            CancelUrl = returnUrl + "?cancel=true",
        };

        var service = new SessionService();
        Session session = await service.CreateAsync(options);

        return session.Url;
    }

    public async Task<SubscriptionResult> CreateSubscriptionAsync(string providerPlanId, string customerEmail)
    {
        // 1. Create or Get Customer
        var customerService = new CustomerService();
        var customerOptions = new CustomerCreateOptions { Email = customerEmail };
        Customer customer;
        
        try 
        {
            var customers = await customerService.ListAsync(new CustomerListOptions { Email = customerEmail, Limit = 1 });
            customer = customers.FirstOrDefault() ?? await customerService.CreateAsync(customerOptions);
        }
        catch 
        {
             customer = await customerService.CreateAsync(customerOptions);
        }

        // 2. Create Checkout Session for Subscription
        var options = new SessionCreateOptions
        {
            Customer = customer.Id,
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    Price = providerPlanId, // Assumes this is a valid Stripe Price ID (e.g., "price_123")
                    Quantity = 1,
                },
            },
            Mode = "subscription",
            SuccessUrl = "http://localhost:5173/payment/success?session_id={CHECKOUT_SESSION_ID}", // TODO: Make configurable
            CancelUrl = "http://localhost:5173/payment/cancel",
        };

        var service = new SessionService();
        Session session = await service.CreateAsync(options);

        // For subscription, we return the Checkout session to the frontend to complete setup
        // But the Interface expects a Subscription Result usually AFTER payment.
        // This is a bit of a mismatch with the "Checkout" flow vs "API-driven" flow.
        // For now, we return the Session URL disguised as success so the frontend redirects.
        
        return new SubscriptionResult
        {
            Success = true,
            ProviderSubscriptionId = session.Id, // Storing Session ID temporarily as we need to redirect
            Status = "PendingRedirect" // Indicating UI needs to redirect to session.Url
        };
    }

    public async Task<bool> VerifyPaymentAsync(string transactionId)
    {
        var service = new SessionService();
        try 
        {
            // transactionId here is treated as Session ID
            var session = await service.GetAsync(transactionId);
            return session.PaymentStatus == "paid";
        }
        catch 
        {
            return false;
        }
    }
}
