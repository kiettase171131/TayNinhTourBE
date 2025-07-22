using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.DataAccessLayer.EntityConfigurations
{
    /// <summary>
    /// Entity Framework configuration cho BankAccount entity
    /// Định nghĩa relationships, constraints và indexes
    /// </summary>
    public class BankAccountConfiguration : IEntityTypeConfiguration<BankAccount>
    {
        public void Configure(EntityTypeBuilder<BankAccount> builder)
        {
            // Primary Key
            builder.HasKey(b => b.Id);

            // Required Properties Configuration
            builder.Property(b => b.UserId)
                .IsRequired();

            builder.Property(b => b.BankName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(b => b.AccountNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(b => b.AccountHolderName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(b => b.IsDefault)
                .IsRequired()
                .HasDefaultValue(false);

            // Optional Properties Configuration
            builder.Property(b => b.Notes)
                .HasMaxLength(500);

            builder.Property(b => b.VerifiedAt)
                .IsRequired(false);

            builder.Property(b => b.VerifiedById)
                .IsRequired(false);

            // Foreign Key Relationships

            // N:1 Relationship with User (Owner)
            builder.HasOne(b => b.User)
                .WithMany(u => u.BankAccounts)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            // N:1 Relationship with User (Verifier) - Optional
            builder.HasOne(b => b.VerifiedBy)
                .WithMany()
                .HasForeignKey(b => b.VerifiedById)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            // 1:N Relationship with WithdrawalRequests
            builder.HasMany(b => b.WithdrawalRequests)
                .WithOne(w => w.BankAccount)
                .HasForeignKey(w => w.BankAccountId)
                .OnDelete(DeleteBehavior.Restrict) // Không cho phép xóa bank account nếu có withdrawal request
                .IsRequired();

            // Unique Constraints
            // Một user không thể có 2 tài khoản ngân hàng giống nhau
            builder.HasIndex(b => new { b.UserId, b.BankName, b.AccountNumber })
                .IsUnique()
                .HasDatabaseName("IX_BankAccount_UserId_BankName_AccountNumber_Unique");

            // Indexes for Performance
            builder.HasIndex(b => b.UserId)
                .HasDatabaseName("IX_BankAccount_UserId");

            builder.HasIndex(b => b.IsDefault)
                .HasDatabaseName("IX_BankAccount_IsDefault");

            builder.HasIndex(b => b.BankName)
                .HasDatabaseName("IX_BankAccount_BankName");

            builder.HasIndex(b => new { b.UserId, b.IsDefault })
                .HasDatabaseName("IX_BankAccount_UserId_IsDefault");

            // Table name
            builder.ToTable("BankAccounts");

            // Check Constraints (if supported by database)
            builder.HasCheckConstraint("CK_BankAccount_AccountNumber_Numeric", 
                "AccountNumber REGEXP '^[0-9]+$'");
        }
    }
}
