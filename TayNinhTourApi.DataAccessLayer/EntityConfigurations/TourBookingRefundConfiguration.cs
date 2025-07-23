using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.DataAccessLayer.EntityConfigurations
{
    /// <summary>
    /// Entity Framework configuration cho TourBookingRefund entity
    /// Định nghĩa relationships, constraints và indexes
    /// </summary>
    public class TourBookingRefundConfiguration : IEntityTypeConfiguration<TourBookingRefund>
    {
        public void Configure(EntityTypeBuilder<TourBookingRefund> builder)
        {
            // Primary Key
            builder.HasKey(r => r.Id);

            // Required Properties Configuration
            builder.Property(r => r.TourBookingId)
                .IsRequired();

            builder.Property(r => r.UserId)
                .IsRequired();

            builder.Property(r => r.RefundType)
                .IsRequired()
                .HasConversion<int>(); // Store enum as int

            builder.Property(r => r.RefundReason)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(r => r.OriginalAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasPrecision(18, 2);

            builder.Property(r => r.RequestedAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasPrecision(18, 2);

            builder.Property(r => r.ApprovedAmount)
                .HasColumnType("decimal(18,2)")
                .HasPrecision(18, 2);

            builder.Property(r => r.ProcessingFee)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasPrecision(18, 2)
                .HasDefaultValue(0);

            builder.Property(r => r.Status)
                .IsRequired()
                .HasDefaultValue(TourRefundStatus.Pending)
                .HasConversion<int>(); // Store enum as int

            builder.Property(r => r.RequestedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            // Optional Properties Configuration
            builder.Property(r => r.ProcessedAt)
                .IsRequired(false);

            builder.Property(r => r.ProcessedById)
                .IsRequired(false);

            builder.Property(r => r.CompletedAt)
                .IsRequired(false);

            builder.Property(r => r.AdminNotes)
                .HasMaxLength(1000);

            builder.Property(r => r.CustomerNotes)
                .HasMaxLength(500);

            builder.Property(r => r.TransactionReference)
                .HasMaxLength(100);

            // Customer Bank Information
            builder.Property(r => r.CustomerBankName)
                .HasMaxLength(100);

            builder.Property(r => r.CustomerAccountNumber)
                .HasMaxLength(50);

            builder.Property(r => r.CustomerAccountHolder)
                .HasMaxLength(100);

            builder.Property(r => r.DaysBeforeTour)
                .IsRequired(false);

            builder.Property(r => r.RefundPercentage)
                .HasColumnType("decimal(5,2)")
                .HasPrecision(5, 2);

            // Computed Properties
            builder.Ignore(r => r.NetRefundAmount); // This will be calculated in code

            // Foreign Key Relationships

            // N:1 Relationship with TourBooking
            builder.HasOne(r => r.TourBooking)
                .WithOne(b => b.RefundRequest)
                .HasForeignKey<TourBookingRefund>(r => r.TourBookingId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            // N:1 Relationship with User (Customer)
            builder.HasOne(r => r.User)
                .WithMany(u => u.TourBookingRefunds)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict) // Không cho phép xóa user nếu có refund request
                .IsRequired();

            // N:1 Relationship with User (Processor) - Optional
            builder.HasOne(r => r.ProcessedBy)
                .WithMany()
                .HasForeignKey(r => r.ProcessedById)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            // Indexes for Performance
            builder.HasIndex(r => r.TourBookingId)
                .IsUnique() // Mỗi booking chỉ có 1 refund request
                .HasDatabaseName("IX_TourBookingRefund_TourBookingId");

            builder.HasIndex(r => r.UserId)
                .HasDatabaseName("IX_TourBookingRefund_UserId");

            builder.HasIndex(r => r.Status)
                .HasDatabaseName("IX_TourBookingRefund_Status");

            builder.HasIndex(r => r.RefundType)
                .HasDatabaseName("IX_TourBookingRefund_RefundType");

            builder.HasIndex(r => r.RequestedAt)
                .HasDatabaseName("IX_TourBookingRefund_RequestedAt");

            builder.HasIndex(r => r.ProcessedAt)
                .HasDatabaseName("IX_TourBookingRefund_ProcessedAt");

            builder.HasIndex(r => r.ProcessedById)
                .HasDatabaseName("IX_TourBookingRefund_ProcessedById");

            // Composite indexes for common queries
            builder.HasIndex(r => new { r.UserId, r.Status })
                .HasDatabaseName("IX_TourBookingRefund_UserId_Status");

            builder.HasIndex(r => new { r.Status, r.RequestedAt })
                .HasDatabaseName("IX_TourBookingRefund_Status_RequestedAt");

            builder.HasIndex(r => new { r.RefundType, r.Status })
                .HasDatabaseName("IX_TourBookingRefund_RefundType_Status");

            builder.HasIndex(r => new { r.ProcessedById, r.ProcessedAt })
                .HasDatabaseName("IX_TourBookingRefund_ProcessedById_ProcessedAt");

            // Table name
            builder.ToTable("TourBookingRefunds");

            // Check Constraints
            builder.HasCheckConstraint("CK_TourBookingRefund_OriginalAmount_Positive", 
                "OriginalAmount > 0");

            builder.HasCheckConstraint("CK_TourBookingRefund_RequestedAmount_NonNegative", 
                "RequestedAmount >= 0");

            builder.HasCheckConstraint("CK_TourBookingRefund_ApprovedAmount_NonNegative", 
                "ApprovedAmount IS NULL OR ApprovedAmount >= 0");

            builder.HasCheckConstraint("CK_TourBookingRefund_ProcessingFee_NonNegative", 
                "ProcessingFee >= 0");

            builder.HasCheckConstraint("CK_TourBookingRefund_RefundPercentage_Valid", 
                "RefundPercentage IS NULL OR (RefundPercentage >= 0 AND RefundPercentage <= 100)");

            builder.HasCheckConstraint("CK_TourBookingRefund_DaysBeforeTour_NonNegative", 
                "DaysBeforeTour IS NULL OR DaysBeforeTour >= 0");

            builder.HasCheckConstraint("CK_TourBookingRefund_ProcessedAt_Logic", 
                "(Status = 0 AND ProcessedAt IS NULL) OR (Status != 0 AND ProcessedAt IS NOT NULL)");

            builder.HasCheckConstraint("CK_TourBookingRefund_CompletedAt_Logic", 
                "(Status != 3 AND CompletedAt IS NULL) OR (Status = 3 AND CompletedAt IS NOT NULL)");
        }
    }
}
