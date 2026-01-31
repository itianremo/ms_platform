using Apps.Domain.Entities;

namespace Apps.Application.Features.Subscriptions.Queries.GetUserSubscriptions;

public class UserSubscriptionDto
{
    public Guid Id { get; set; }
    public Guid AppId { get; set; }
    public Guid PackageId { get; set; }
    public string PackageName { get; set; } // Enriched
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public decimal PricePaid { get; set; }
}
