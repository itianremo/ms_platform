using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Persistence.Configurations;

public class UserAppMembershipConfiguration : IEntityTypeConfiguration<UserAppMembership>
{
    public void Configure(EntityTypeBuilder<UserAppMembership> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.UserId)
            .IsRequired();

        builder.Property(m => m.AppId)
            .IsRequired();

        builder.Property(m => m.Status)
            .HasConversion<string>();

        // Relationship with Role
        builder.HasOne(m => m.Role)
            .WithMany() // Role can have many memberships? Or maybe explicitly mapped
            .HasForeignKey(m => m.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique constraint: One membership per User per App
        builder.HasIndex(m => new { m.UserId, m.AppId })
            .IsUnique();
    }
}
