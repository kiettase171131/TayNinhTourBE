using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.DataAccessLayer.EntityConfigurations
{
    public class AIChatSessionConfiguration : IEntityTypeConfiguration<AIChatSession>
    {
        public void Configure(EntityTypeBuilder<AIChatSession> builder)
        {
            builder.ToTable("AIChatSessions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.UserId)
                .IsRequired();

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Active");

            // B? DefaultValueSql cho LastMessageAt vì MySQL không h? tr? CURRENT_TIMESTAMP cho datetime(6)
            builder.Property(x => x.LastMessageAt)
                .IsRequired();

            // Relationships
            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Messages)
                .WithOne(x => x.Session)
                .HasForeignKey(x => x.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.LastMessageAt);
            builder.HasIndex(x => x.CreatedAt);
        }
    }
}