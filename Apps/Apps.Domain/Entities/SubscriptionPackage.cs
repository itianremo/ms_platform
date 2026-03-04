using Shared.Kernel;

namespace Apps.Domain.Entities;

public enum PackageType
{
    Subscription = 0,
    Consumable = 1 // e.g. Coins
}

public class SubscriptionPackage : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Discount { get; private set; }
    public SubscriptionPeriod Period { get; private set; }
    public Guid AppId { get; private set; }
    public bool IsActive { get; private set; }
    
    public PackageType Type { get; private set; }
    public int CoinsAmount { get; private set; }
    public string LocalizedPricingJson { get; private set; } = "{}";

    private SubscriptionPackage() { }

    public SubscriptionPackage(Guid appId, string name, string description, decimal discount, SubscriptionPeriod period, PackageType type = PackageType.Subscription, int coinsAmount = 0, string localizedPricingJson = "{}")
    {
        Id = Guid.NewGuid();
        AppId = appId;
        Name = name;
        Description = description;
        Discount = discount;
        Period = period;
        IsActive = true;
        Type = type;
        CoinsAmount = coinsAmount;
        LocalizedPricingJson = localizedPricingJson;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
    
    public void UpdatePricing(string localizedPricingJson)
    {
        LocalizedPricingJson = localizedPricingJson;
    }
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
