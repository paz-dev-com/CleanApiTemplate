using CleanApiTemplate.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanApiTemplate.Data.Persistence.Configurations;

/// <summary>
/// Entity configuration for Category
/// </summary>
public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.Property(c => c.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(c => c.UpdatedBy)
            .HasMaxLength(256);

        builder.Property(c => c.DeletedBy)
            .HasMaxLength(256);

        builder.Property(c => c.RowVersion)
            .IsRowVersion();
    }
}