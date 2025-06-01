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

        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string AuthorName { get; set; } = null!;

        
        public virtual ICollection<BlogImage> BlogImages { get; set; } = new List<BlogImage>();
    }
}
