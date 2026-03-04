using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Persistence.Configurations;

public class UserAppMembershipConfiguration : IEntityTypeConfiguration<UserAppMembership>
{
    public void Configure(EntityTypeBuilder<UserAppMembership> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.UserId).IsRequired();
        builder.Property(m => m.AppId).IsRequired();
        builder.Property(m => m.RoleId).IsRequired();
        builder.Property(m => m.Status).IsRequired();
        // UserSubscriptionId is nullable, LastLoginUtc is nullable

        builder.HasIndex(m => m.RoleId);
        builder.HasIndex(m => new { m.UserId, m.AppId }).IsUnique();

        // Foreign keys mapping to Users. (Role is technically linked but often loosely coupled depending on implementation)
        builder.HasOne(m => m.User)
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(m => m.Role)
            .WithMany()
            .HasForeignKey(m => m.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
