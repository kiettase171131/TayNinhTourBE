using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.DataAccessLayer.Configurations
{
    public class TourConfiguration : IEntityTypeConfiguration<Tour>
    {
        public void Configure(EntityTypeBuilder<Tour> builder)
        {
            // Tour - User relationship
            builder.HasOne(t => t.CreatedBy)
                .WithMany(u => u.ToursCreated)
                .HasForeignKey(t => t.CreatedById)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(t => t.UpdatedBy)
                .WithMany(u => u.ToursUpdated)
                .HasForeignKey(t => t.UpdatedById)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
