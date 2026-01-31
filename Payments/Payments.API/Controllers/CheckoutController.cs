using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using Microsoft.Extensions.Configuration;
using Payments.Domain.Entities; // Assuming Package/Subscription entities are shared or DTOs used
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Payments.API.Controllers;

[Route("api/payments")]
[ApiController]
public class CheckoutController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<CheckoutController> _logger;

    public CheckoutController(IConfiguration configuration, ILogger<CheckoutController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> CreateCheckoutSession([FromBody] CheckoutRequest request)
    {
        var domain = _configuration["FrontendUrl"] ?? "http://localhost:3000";

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(request.Amount * 100), // Amount in cents
                        Currency = request.Currency,
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = request.PackageName,
                        },
                    },
                    Quantity = 1,
                },
            },
            Mode = "payment", // Or "subscription" if using Stripe Subscriptions directly
            SuccessUrl = domain + "/payment/success?session_id={CHECKOUT_SESSION_ID}",
            CancelUrl = domain + "/payment/cancel",
            Metadata = new Dictionary<string, string>
            {
                { "UserId", request.UserId.ToString() },
                { "AppId", request.AppId.ToString() },
                { "PackageId", request.PackageId.ToString() }
            }
        };

        var service = new SessionService();
        Session session = await service.CreateAsync(options);

        return Ok(new { url = session.Url });
    }
}

public class CheckoutRequest
{
    public Guid UserId { get; set; }
    public Guid AppId { get; set; }
    public Guid PackageId { get; set; }
    public string PackageName { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "usd";
}
