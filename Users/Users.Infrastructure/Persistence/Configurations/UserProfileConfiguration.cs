using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Users.Domain.Entities;

namespace Users.Infrastructure.Persistence.Configurations;

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.DisplayName).IsRequired().HasMaxLength(100);
        
        // Composite Index for Uniqueness per App
        builder.HasIndex(p => new { p.UserId, p.AppId }).IsUnique();
    }
}
