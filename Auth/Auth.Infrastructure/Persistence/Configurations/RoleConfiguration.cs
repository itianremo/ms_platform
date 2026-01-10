using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.AppId)
            .IsRequired();

        // Many-to-Many with Permission
        builder.HasMany(r => r.Permissions)
            .WithMany()
            .UsingEntity(j => j.ToTable("RolePermissions"));
            
        builder.HasIndex(r => new { r.AppId, r.Name })
            .IsUnique();
    }
}
