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
    public class SupportTicketConfiguration : IEntityTypeConfiguration<SupportTicket>
    {
        public void Configure(EntityTypeBuilder<SupportTicket> builder)
        {
            builder.HasKey(t => t.Id);
            builder
                .HasOne(t => t.User)
                .WithMany(u => u.TicketsCreated)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict); 

         
            builder
                .HasOne(t => t.Admin)
                .WithMany(u => u.TicketsAssigned)
                .HasForeignKey(t => t.AdminId)
                .OnDelete(DeleteBehavior.Restrict);

            // 3. Quan hệ SupportTicket ↔ SupportTicketComment
            builder
                .HasMany(t => t.Comments)
                .WithOne(c => c.SupportTicket)
                .HasForeignKey(c => c.SupportTicketId)
                .OnDelete(DeleteBehavior.Cascade);

            // 4. Quan hệ SupportTicket ↔ SupportTicketImage
            builder
                .HasMany(t => t.Images)
                .WithOne(i => i.SupportTicket)
                .HasForeignKey(i => i.SupportTicketId)
                .OnDelete(DeleteBehavior.Cascade);

            
        }
    }
}
