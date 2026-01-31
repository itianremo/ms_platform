using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;
using MassTransit;
using Shared.Messaging.Events;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace Payments.API.Controllers;

[Route("api/webhooks")]
[ApiController]
public class StripeWebhookController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripeWebhookController> _logger;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly Microsoft.Extensions.Caching.Distributed.IDistributedCache _cache;

    public StripeWebhookController(IConfiguration configuration, ILogger<StripeWebhookController> logger, IPublishEndpoint publishEndpoint, Microsoft.Extensions.Caching.Distributed.IDistributedCache cache)
    {
        _configuration = configuration;
        _logger = logger;
        _publishEndpoint = publishEndpoint;
        _cache = cache;
    }

    [HttpPost("stripe")]
    public async Task<IActionResult> Handle()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"];
        var webhookSecret = _configuration["StripeSettings:WebhookSecret"];

        try
        {
            var stripeEvent = EventUtility.ConstructEvent(json, signature, webhookSecret);
            
            // Idempotency Check
            var cacheKey = $"processed_event:{stripeEvent.Id}";
            var isProcessed = await _cache.GetStringAsync(cacheKey);
            
            if (!string.IsNullOrEmpty(isProcessed))
            {
                _logger.LogInformation("Event {EventId} already processed.", stripeEvent.Id);
                return Ok();
            }

            if (stripeEvent.Type == "invoice.payment_succeeded")
            {
                var invoice = stripeEvent.Data.Object as Invoice;
                
                if (invoice != null)
                {
                    _logger.LogInformation("Stripe Invoice Payment Succeeded: {Id}", invoice.Id);

                    // Extract Metadata
                    // Try to get metadata directly from Invoice
                    var userIdStr = invoice.Metadata?.ContainsKey("UserId") == true ? invoice.Metadata["UserId"] : null;
                    var appIdStr = invoice.Metadata?.ContainsKey("AppId") == true ? invoice.Metadata["AppId"] : null;
                    var packageIdStr = invoice.Metadata?.ContainsKey("PackageId") == true ? invoice.Metadata["PackageId"] : null;

                    if (Guid.TryParse(userIdStr, out var userId) && 
                        Guid.TryParse(appIdStr, out var appId) && 
                        Guid.TryParse(packageIdStr, out var packageId))
                    {
                        await _publishEndpoint.Publish(new PaymentSucceededEvent(
                            userId,
                            appId,
                            packageId,
                            invoice.Id,
                            (decimal)invoice.AmountPaid / 100m,
                            invoice.Currency,
                            json
                        ));
                    }
                    else
                    {
                        _logger.LogWarning("Missing Metadata in Stripe Invoice: {Id}", invoice.Id);
                    }
                }
            }
            else if (stripeEvent.Type == "checkout.session.completed")
            {
                 var session = stripeEvent.Data.Object as Session; // Session from Stripe.Checkout namespace
                 // Handle One-time payments or initial subscription checkout
                 if (session != null)
                 {
                    _logger.LogInformation("Stripe Checkout Session Completed: {Id}", session.Id);
                    
                    if (session.Metadata != null && 
                        session.Metadata.ContainsKey("UserId") && 
                        session.Metadata.ContainsKey("AppId") && 
                        session.Metadata.ContainsKey("PackageId"))
                    {
                        await _publishEndpoint.Publish(new PaymentSucceededEvent(
                            Guid.Parse(session.Metadata["UserId"]),
                            Guid.Parse(session.Metadata["AppId"]),
                            Guid.Parse(session.Metadata["PackageId"]),
                            session.Id,
                            (decimal)(session.AmountTotal ?? 0) / 100m,
                            session.Currency,
                            json
                        ));
                    }
                 }
            }
            
            // Mark as Processed (24h expiry)
            await _cache.SetStringAsync(cacheKey, "true", new Microsoft.Extensions.Caching.Distributed.DistributedCacheEntryOptions 
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
            });

            return Ok();
        }
        catch (StripeException e)
        {
            _logger.LogError(e, "Stripe Webhook Error");
            return BadRequest();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "General Webhook Error");
            return StatusCode(500);
        }
    }
}
