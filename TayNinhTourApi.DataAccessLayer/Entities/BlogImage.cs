using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.DataAccessLayer.Entities
{
    public class BlogImage : BaseEntity
    {
        public Guid BlogId { get; set; }
        public virtual Blog Blog { get; set; } = null!;
        public string Url { get; set; } = null!;
    }
}
