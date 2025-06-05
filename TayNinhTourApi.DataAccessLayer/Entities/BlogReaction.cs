using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.DataAccessLayer.Entities
{
    public enum BlogStatusEnum
    {
        None,
        Like,
        Dislike,
    }
    public class BlogReaction : BaseEntity
    {
        public Guid BlogId { get; set; }
        public virtual Blog Blog { get; set; } = null!;

        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;

        
        public BlogStatusEnum Reaction { get; set; } = BlogStatusEnum.None;
    }
}
