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

    public SubscriptionPackage(Guid appId, string name, string description, decimal price, decimal discount, SubscriptionPeriod period)
    {
        Id = Guid.NewGuid();
        AppId = appId;
        Name = name;
        Description = description;
        Price = price;
        Discount = discount;
        Currency = "USD"; // Default for now
        Period = period;
        IsActive = true;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}

public enum SubscriptionPeriod
{
    Weekly = 1,
    Monthly = 2,
    Quarterly = 3,
    SemiAnnually = 6,
    Yearly = 12,
    Unlimited = 99
}
