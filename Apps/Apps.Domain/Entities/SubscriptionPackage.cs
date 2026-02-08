using Shared.Kernel;

namespace Apps.Domain.Entities;

public class SubscriptionPackage : Entity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal Price { get; private set; }
    public decimal Discount { get; private set; }
    public string Currency { get; private set; }
    public SubscriptionPeriod Period { get; private set; }
    public Guid AppId { get; private set; }
    public bool IsActive { get; private set; }

    private SubscriptionPackage() { }

    public SubscriptionPackage(Guid appId, string name, string description, decimal price, decimal discount, SubscriptionPeriod period, string currency = "USD")
    {
        Id = Guid.NewGuid();
        AppId = appId;
        Name = name;
        Description = description;
        Price = price;
        Discount = discount;
        Currency = currency;
        Period = period;
        IsActive = true;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}

public enum SubscriptionPeriod
{
    Weekly = 7,  // Days
    BiWeekly = 14, // Days
    Monthly = 30, // Days (Approx)
    Quarterly = 90, // Days (Approx)
    SemiAnnually = 180, // Days (Approx)
    Yearly = 365, // Days
    Unlimited = 9999 // Special
}
