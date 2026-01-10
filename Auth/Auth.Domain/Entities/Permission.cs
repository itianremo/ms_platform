using Shared.Kernel;

namespace Auth.Domain.Entities;

public class Permission : Entity
{
    public string Name { get; private set; } // e.g., "users.create", "dashboard.access"
    public string Description { get; private set; }

    private Permission() { }

    public Permission(string name, string description)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
    }
}
