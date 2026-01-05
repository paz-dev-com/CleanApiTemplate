using CleanApiTemplate.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanApiTemplate.Data.Persistence.Configurations;

/// <summary>
/// Entity configuration for UserRole (many-to-many)
/// </summary>
public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles");

        builder.HasKey(ur => ur.Id);

        // Configure relationships
        builder.HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(ur => ur.AssignedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(ur => ur.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(ur => ur.UpdatedBy)
            .HasMaxLength(256);

        builder.Property(ur => ur.DeletedBy)
            .HasMaxLength(256);

        builder.Property(ur => ur.RowVersion)
            .IsRowVersion();

        // Composite index to prevent duplicate role assignments
        builder.HasIndex(ur => new { ur.UserId, ur.RoleId })
            .IsUnique()
            .HasDatabaseName("IX_UserRoles_UserId_RoleId");
    }
}