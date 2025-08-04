using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.DataAccessLayer.EntityConfigurations
{
    /// <summary>
    /// Entity Framework configuration cho PaymentTransaction entity
    /// </summary>
    public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
    {
        public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
        {
            // Table Configuration
            builder.ToTable("PaymentTransactions", t =>
            {
                t.HasCheckConstraint("CK_PaymentTransactions_Amount_Positive", "Amount > 0");
                t.HasCheckConstraint("CK_PaymentTransactions_OrderOrBooking", 
                    "(OrderId IS NOT NULL AND TourBookingId IS NULL) OR (OrderId IS NULL AND TourBookingId IS NOT NULL)");
            });

            // Primary Key
            builder.HasKey(pt => pt.Id);

            // Property Configurations
            builder.Property(pt => pt.OrderId)
                .IsRequired(false)
                .HasComment("ID của Order (cho product payment)");

            builder.Property(pt => pt.TourBookingId)
                .IsRequired(false)
                .HasComment("ID của TourBooking (cho tour booking payment)");

            builder.Property(pt => pt.Amount)
                .IsRequired()
                .HasPrecision(18, 2)
                .HasComment("Số tiền giao dịch");

            builder.Property(pt => pt.Status)
                .IsRequired()
                .HasConversion<int>()
                .HasComment("Trạng thái giao dịch");

            builder.Property(pt => pt.Description)
                .HasMaxLength(500)
                .HasComment("Mô tả giao dịch");

            builder.Property(pt => pt.ExpiredAt)
                .IsRequired(false)
                .HasComment("Thời gian hết hạn giao dịch");

            builder.Property(pt => pt.Gateway)
                .IsRequired()
                .HasConversion<int>()
                .HasComment("Cổng thanh toán sử dụng");

            builder.Property(pt => pt.PayOsOrderCode)
                .IsRequired(false)
                .HasComment("PayOS Order Code (số)");

            builder.Property(pt => pt.PayOsTransactionId)
                .HasMaxLength(100)
                .IsRequired(false)
                .HasComment("PayOS Transaction ID");

            builder.Property(pt => pt.CheckoutUrl)
                .HasMaxLength(1000)
                .IsRequired(false)
                .HasComment("URL checkout PayOS");

            builder.Property(pt => pt.QrCode)
                .HasMaxLength(1000)
                .IsRequired(false)
                .HasComment("QR Code data từ PayOS");

            builder.Property(pt => pt.FailureReason)
                .HasMaxLength(500)
                .IsRequired(false)
                .HasComment("Lý do thất bại (nếu có)");

            builder.Property(pt => pt.ParentTransactionId)
                .IsRequired(false)
                .HasComment("ID của transaction cha (cho retry chain)");

            builder.Property(pt => pt.WebhookPayload)
                .HasColumnType("longtext")
                .IsRequired(false)
                .HasComment("Webhook payload từ PayOS (JSON)");

            // Foreign Key Relationships
            builder.HasOne(pt => pt.Order)
                .WithMany()
                .HasForeignKey(pt => pt.OrderId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);

            builder.HasOne(pt => pt.TourBooking)
                .WithMany()
                .HasForeignKey(pt => pt.TourBookingId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);

            builder.HasOne(pt => pt.ParentTransaction)
                .WithMany(pt => pt.ChildTransactions)
                .HasForeignKey(pt => pt.ParentTransactionId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // Indexes for Performance
            builder.HasIndex(pt => pt.OrderId)
                .HasDatabaseName("IX_PaymentTransactions_OrderId");

            builder.HasIndex(pt => pt.TourBookingId)
                .HasDatabaseName("IX_PaymentTransactions_TourBookingId");

            builder.HasIndex(pt => pt.Status)
                .HasDatabaseName("IX_PaymentTransactions_Status");

            builder.HasIndex(pt => pt.Gateway)
                .HasDatabaseName("IX_PaymentTransactions_Gateway");

            builder.HasIndex(pt => pt.PayOsOrderCode)
                .HasDatabaseName("IX_PaymentTransactions_PayOsOrderCode");

            builder.HasIndex(pt => pt.PayOsTransactionId)
                .HasDatabaseName("IX_PaymentTransactions_PayOsTransactionId");

            builder.HasIndex(pt => pt.ParentTransactionId)
                .HasDatabaseName("IX_PaymentTransactions_ParentTransactionId");

            builder.HasIndex(pt => pt.ExpiredAt)
                .HasDatabaseName("IX_PaymentTransactions_ExpiredAt");

            // Composite indexes for common queries
            builder.HasIndex(pt => new { pt.OrderId, pt.Gateway, pt.Status })
                .HasDatabaseName("IX_PaymentTransactions_Order_Gateway_Status");

            builder.HasIndex(pt => new { pt.TourBookingId, pt.Gateway, pt.Status })
                .HasDatabaseName("IX_PaymentTransactions_TourBooking_Gateway_Status");

            builder.HasIndex(pt => new { pt.Status, pt.CreatedAt })
                .HasDatabaseName("IX_PaymentTransactions_Status_CreatedAt");
        }
    }
}
