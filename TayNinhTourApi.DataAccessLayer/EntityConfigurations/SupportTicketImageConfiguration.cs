using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.DataAccessLayer.EntityConfigurations
{
    public class SupportTicketImageConfiguration : IEntityTypeConfiguration<SupportTicketImage>
    {
        public void Configure(EntityTypeBuilder<SupportTicketImage> builder)
        {
            builder.HasKey(i => i.Id);

            // Mỗi image thuộc về một SupportTicket
            builder
                .HasOne(i => i.SupportTicket)
                .WithMany(t => t.Images)
                .HasForeignKey(i => i.SupportTicketId)
                .OnDelete(DeleteBehavior.Cascade);

            
        }
    }
}
