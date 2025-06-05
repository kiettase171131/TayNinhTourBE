using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.DataAccessLayer.Entities
{
    public class BlogComment : BaseEntity
    {
        
        public string Content { get; set; } = null!;

        
        public Guid BlogId { get; set; }
        public virtual Blog Blog { get; set; } = null!;

        
        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;

    
        public Guid? ParentCommentId { get; set; }
        public virtual BlogComment? ParentComment { get; set; }

        
        public virtual ICollection<BlogComment> Replies { get; set; } = new List<BlogComment>();
    }
}
