using Shared.Kernel;

namespace Auth.Domain.Entities;

public class Role : Entity
{
    public Guid AppId { get; private set; } // Role is scoped to an App
    public string Name { get; private set; }
    public bool IsSealed { get; private set; } // Cannot be deleted or modified
    
    // Many-to-Many relation with Permission
    public virtual ICollection<Permission> Permissions { get; private set; } = new List<Permission>();

    private Role() { }

    public Role(Guid appId, string name)
    {
        Id = Guid.NewGuid();
        AppId = appId;
        Name = name;
    }

    public void AddPermission(Permission permission)
    {
        if (!Permissions.Contains(permission))
        {
            Permissions.Add(permission);
        }
    }

    public void MarkAsSealed()
    {
        IsSealed = true;
    }
}
