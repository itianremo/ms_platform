using Shared.Kernel;

namespace Apps.Domain.Entities;

public class UserSubscription : Entity
{
    public Guid UserId { get; private set; }
    public Guid AppId { get; private set; }
    public Guid PackageId { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public bool IsActive { get; private set; }

    // Snapshot details at time of purchase
    public decimal PricePaid { get; private set; }
    public string FeaturesSnapshot { get; private set; }
    
    // Default constructor for EF Core
    private UserSubscription() 
    {
        FeaturesSnapshot = string.Empty;
    }

    public UserSubscription(Guid userId, Guid appId, Guid packageId, DateTime startDate, DateTime endDate, decimal pricePaid, string featuresSnapshot)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        AppId = appId;
        PackageId = packageId;
        StartDate = startDate;
        EndDate = endDate;
        PricePaid = pricePaid;
        FeaturesSnapshot = featuresSnapshot;
        IsActive = true;
    }

    public void Cancel()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public bool IsValid()
    {
        return IsActive && DateTime.UtcNow >= StartDate && DateTime.UtcNow <= EndDate;
    }

    public void ExpireNow()
    {
        EndDate = DateTime.UtcNow.AddDays(-1);
        IsActive = false;
    }
}
