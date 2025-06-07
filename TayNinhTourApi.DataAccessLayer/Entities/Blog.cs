using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.DataAccessLayer.Entities
{
    public class Blog : BaseEntity
    {

        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;
        public byte Status { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string AuthorName { get; set; } = null!;
        public string? CommentOfAdmin { get; set; } 


        public virtual ICollection<BlogImage> BlogImages { get; set; } = new List<BlogImage>();
        public virtual ICollection<BlogReaction> BlogReactions { get; set; } = new List<BlogReaction>();
        public virtual ICollection<BlogComment> BlogComments { get; set; } = new List<BlogComment>();

    }
}
