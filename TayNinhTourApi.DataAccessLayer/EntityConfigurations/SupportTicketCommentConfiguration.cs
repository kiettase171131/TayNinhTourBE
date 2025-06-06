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
    public class SupportTicketCommentConfiguration : IEntityTypeConfiguration<SupportTicketComment>
    {
        public void Configure(EntityTypeBuilder<SupportTicketComment> builder)
        {
            builder.HasKey(c => c.Id);

            
            builder
                .HasOne(c => c.SupportTicket)
                .WithMany(t => t.Comments)
                .HasForeignKey(c => c.SupportTicketId)
                .OnDelete(DeleteBehavior.Cascade);

            // 2. Mỗi comment do một User tạo: 
            //    - FK: CreatedById
            //    - Chiều ngược: User.TicketComments
            builder
                .HasOne(c => c.CreatedBy)
                .WithMany(u => u.TicketComments)
                .HasForeignKey(c => c.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            
        }
    }
}
