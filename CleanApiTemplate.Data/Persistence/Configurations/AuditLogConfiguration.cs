using CleanApiTemplate.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanApiTemplate.Data.Persistence.Configurations;

/// <summary>
/// Entity configuration for AuditLog
/// </summary>
public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.EntityName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(a => a.EntityId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(a => a.Action)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.PerformedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(a => a.IpAddress)
            .HasMaxLength(45); // IPv6 max length

        builder.Property(a => a.OldValues)
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.NewValues)
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.ChangedProperties)
            .HasMaxLength(2000);

        builder.Property(a => a.Metadata)
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(a => a.UpdatedBy)
            .HasMaxLength(256);

        builder.Property(a => a.DeletedBy)
            .HasMaxLength(256);

        // AuditLogs should never be deleted, but we keep the structure consistent
        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}