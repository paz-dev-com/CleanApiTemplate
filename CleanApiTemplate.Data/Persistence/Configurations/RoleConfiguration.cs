using CleanApiTemplate.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanApiTemplate.Data.Persistence.Configurations;

/// <summary>
/// Entity configuration for Role
/// </summary>
public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.NormalizedName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.Description)
            .HasMaxLength(500);

        builder.Property(r => r.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(r => r.UpdatedBy)
            .HasMaxLength(256);

        builder.Property(r => r.DeletedBy)
            .HasMaxLength(256);

        builder.Property(r => r.RowVersion)
            .IsRowVersion();

        // Unique index on normalized name
        builder.HasIndex(r => r.NormalizedName)
            .IsUnique()
            .HasDatabaseName("IX_Roles_NormalizedName");
    }
}