using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.DataAccessLayer.EntityConfigurations
{
    public class AIChatMessageConfiguration : IEntityTypeConfiguration<AIChatMessage>
    {
        public void Configure(EntityTypeBuilder<AIChatMessage> builder)
        {
            builder.ToTable("AIChatMessages");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.SessionId)
                .IsRequired();

            builder.Property(x => x.Content)
                .IsRequired()
                .HasMaxLength(4000);

            builder.Property(x => x.MessageType)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(x => x.TokensUsed)
                .IsRequired(false);

            builder.Property(x => x.ResponseTimeMs)
                .IsRequired(false);

            builder.Property(x => x.Metadata)
                .HasMaxLength(1000)
                .IsRequired(false);

            // Relationships
            builder.HasOne(x => x.Session)
                .WithMany(x => x.Messages)
                .HasForeignKey(x => x.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(x => x.SessionId);
            builder.HasIndex(x => x.MessageType);
            builder.HasIndex(x => x.CreatedAt);
        }
    }
}