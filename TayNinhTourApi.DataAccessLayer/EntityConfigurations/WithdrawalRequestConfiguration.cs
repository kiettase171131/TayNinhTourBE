using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.DataAccessLayer.EntityConfigurations
{
    /// <summary>
    /// Entity Framework configuration cho WithdrawalRequest entity
    /// Định nghĩa relationships, constraints và indexes
    /// </summary>
    public class WithdrawalRequestConfiguration : IEntityTypeConfiguration<WithdrawalRequest>
    {
        public void Configure(EntityTypeBuilder<WithdrawalRequest> builder)
        {
            // Primary Key
            builder.HasKey(w => w.Id);

            // Required Properties Configuration
            builder.Property(w => w.UserId)
                .IsRequired();

            builder.Property(w => w.BankAccountId)
                .IsRequired();

            builder.Property(w => w.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasPrecision(18, 2);

            builder.Property(w => w.Status)
                .IsRequired()
                .HasDefaultValue(WithdrawalStatus.Pending)
                .HasConversion<int>(); // Store enum as int

            builder.Property(w => w.RequestedAt)
    .IsRequired()
    .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            builder.Property(w => w.WalletBalanceAtRequest)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasPrecision(18, 2);

            builder.Property(w => w.WithdrawalFee)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasPrecision(18, 2)
                .HasDefaultValue(0);

            // Optional Properties Configuration
            builder.Property(w => w.ProcessedAt)
                .IsRequired(false);

            builder.Property(w => w.ProcessedById)
                .IsRequired(false);

            builder.Property(w => w.AdminNotes)
                .HasMaxLength(1000);

            builder.Property(w => w.UserNotes)
                .HasMaxLength(500);

            builder.Property(w => w.TransactionReference)
                .HasMaxLength(100);

            // Computed Properties
            builder.Ignore(w => w.NetAmount); // This will be calculated in code

            // Foreign Key Relationships

            // N:1 Relationship with User (Requester)
            builder.HasOne(w => w.User)
                .WithMany(u => u.WithdrawalRequests)
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            // N:1 Relationship with BankAccount
            builder.HasOne(w => w.BankAccount)
                .WithMany(b => b.WithdrawalRequests)
                .HasForeignKey(w => w.BankAccountId)
                .OnDelete(DeleteBehavior.Restrict) // Không cho phép xóa bank account nếu có withdrawal request
                .IsRequired();

            // N:1 Relationship with User (Processor) - Optional
            builder.HasOne(w => w.ProcessedBy)
                .WithMany()
                .HasForeignKey(w => w.ProcessedById)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            // Indexes for Performance
            builder.HasIndex(w => w.UserId)
                .HasDatabaseName("IX_WithdrawalRequest_UserId");

            builder.HasIndex(w => w.Status)
                .HasDatabaseName("IX_WithdrawalRequest_Status");

            builder.HasIndex(w => w.RequestedAt)
                .HasDatabaseName("IX_WithdrawalRequest_RequestedAt");

            builder.HasIndex(w => w.ProcessedAt)
                .HasDatabaseName("IX_WithdrawalRequest_ProcessedAt");

            builder.HasIndex(w => w.BankAccountId)
                .HasDatabaseName("IX_WithdrawalRequest_BankAccountId");

            builder.HasIndex(w => w.ProcessedById)
                .HasDatabaseName("IX_WithdrawalRequest_ProcessedById");

            // Composite indexes for common queries
            builder.HasIndex(w => new { w.UserId, w.Status })
                .HasDatabaseName("IX_WithdrawalRequest_UserId_Status");

            builder.HasIndex(w => new { w.Status, w.RequestedAt })
                .HasDatabaseName("IX_WithdrawalRequest_Status_RequestedAt");

            builder.HasIndex(w => new { w.ProcessedById, w.ProcessedAt })
                .HasDatabaseName("IX_WithdrawalRequest_ProcessedById_ProcessedAt");

            // Table name
            builder.ToTable("WithdrawalRequests");

            // Check Constraints
            builder.HasCheckConstraint("CK_WithdrawalRequest_Amount_Positive", 
                "Amount > 0");

            builder.HasCheckConstraint("CK_WithdrawalRequest_WalletBalance_NonNegative", 
                "WalletBalanceAtRequest >= 0");

            builder.HasCheckConstraint("CK_WithdrawalRequest_WithdrawalFee_NonNegative", 
                "WithdrawalFee >= 0");

            builder.HasCheckConstraint("CK_WithdrawalRequest_ProcessedAt_Logic", 
                "(Status = 0 AND ProcessedAt IS NULL) OR (Status != 0 AND ProcessedAt IS NOT NULL)");
        }
    }
}
