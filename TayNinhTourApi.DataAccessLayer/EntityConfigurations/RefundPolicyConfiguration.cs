using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.DataAccessLayer.EntityConfigurations
{
    /// <summary>
    /// Entity Framework configuration cho RefundPolicy entity
    /// Định nghĩa relationships, constraints và indexes
    /// </summary>
    public class RefundPolicyConfiguration : IEntityTypeConfiguration<RefundPolicy>
    {
        public void Configure(EntityTypeBuilder<RefundPolicy> builder)
        {
            // Primary Key
            builder.HasKey(p => p.Id);

            // Required Properties Configuration
            builder.Property(p => p.RefundType)
                .IsRequired()
                .HasConversion<int>(); // Store enum as int

            builder.Property(p => p.MinDaysBeforeEvent)
                .IsRequired();

            builder.Property(p => p.MaxDaysBeforeEvent)
                .IsRequired(false);

            builder.Property(p => p.RefundPercentage)
                .IsRequired()
                .HasColumnType("decimal(5,2)")
                .HasPrecision(5, 2);

            builder.Property(p => p.ProcessingFee)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasPrecision(18, 2)
                .HasDefaultValue(0);

            builder.Property(p => p.ProcessingFeePercentage)
                .IsRequired()
                .HasColumnType("decimal(5,2)")
                .HasPrecision(5, 2)
                .HasDefaultValue(0);

            builder.Property(p => p.Description)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(p => p.Priority)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(p => p.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(p => p.EffectiveFrom)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            builder.Property(p => p.EffectiveTo)
                .IsRequired(false);

            builder.Property(p => p.InternalNotes)
                .HasMaxLength(1000);

            // Indexes for Performance
            builder.HasIndex(p => p.RefundType)
                .HasDatabaseName("IX_RefundPolicy_RefundType");

            builder.HasIndex(p => p.IsActive)
                .HasDatabaseName("IX_RefundPolicy_IsActive");

            builder.HasIndex(p => p.Priority)
                .HasDatabaseName("IX_RefundPolicy_Priority");

            builder.HasIndex(p => p.EffectiveFrom)
                .HasDatabaseName("IX_RefundPolicy_EffectiveFrom");

            builder.HasIndex(p => p.EffectiveTo)
                .HasDatabaseName("IX_RefundPolicy_EffectiveTo");

            // Composite indexes for common queries
            builder.HasIndex(p => new { p.RefundType, p.IsActive })
                .HasDatabaseName("IX_RefundPolicy_RefundType_IsActive");

            builder.HasIndex(p => new { p.RefundType, p.IsActive, p.Priority })
                .HasDatabaseName("IX_RefundPolicy_RefundType_IsActive_Priority");

            builder.HasIndex(p => new { p.IsActive, p.EffectiveFrom, p.EffectiveTo })
                .HasDatabaseName("IX_RefundPolicy_Active_Effective");

            builder.HasIndex(p => new { p.RefundType, p.MinDaysBeforeEvent, p.MaxDaysBeforeEvent })
                .HasDatabaseName("IX_RefundPolicy_RefundType_DaysRange");

            // Table name
            builder.ToTable("RefundPolicies");

            // Check Constraints
            builder.HasCheckConstraint("CK_RefundPolicy_MinDaysBeforeEvent_NonNegative", 
                "MinDaysBeforeEvent >= 0");

            builder.HasCheckConstraint("CK_RefundPolicy_MaxDaysBeforeEvent_Valid", 
                "MaxDaysBeforeEvent IS NULL OR MaxDaysBeforeEvent >= MinDaysBeforeEvent");

            builder.HasCheckConstraint("CK_RefundPolicy_RefundPercentage_Valid", 
                "RefundPercentage >= 0 AND RefundPercentage <= 100");

            builder.HasCheckConstraint("CK_RefundPolicy_ProcessingFee_NonNegative", 
                "ProcessingFee >= 0");

            builder.HasCheckConstraint("CK_RefundPolicy_ProcessingFeePercentage_Valid", 
                "ProcessingFeePercentage >= 0 AND ProcessingFeePercentage <= 100");

            builder.HasCheckConstraint("CK_RefundPolicy_Priority_Valid", 
                "Priority >= 1 AND Priority <= 100");

            builder.HasCheckConstraint("CK_RefundPolicy_EffectiveTo_Logic", 
                "EffectiveTo IS NULL OR EffectiveTo > EffectiveFrom");

            // Unique constraint to prevent overlapping policies
            builder.HasIndex(p => new { p.RefundType, p.MinDaysBeforeEvent, p.MaxDaysBeforeEvent, p.IsActive })
                .IsUnique()
                .HasDatabaseName("IX_RefundPolicy_Unique_Range")
                .HasFilter("IsActive = 1"); // Only active policies need to be unique
        }
    }
}
