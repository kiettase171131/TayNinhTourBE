using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Blog
{
    public class ResponseCommentDto : BaseResposeDto
    {
        public Guid Id { get; set; }
        public Guid BlogId { get; set; }

        // Nếu là reply, ParentCommentId != null
        public Guid? ParentCommentId { get; set; }

        public Guid UserId { get; set; }
        public string UserName { get; set; } = null!; // Tên hiển thị của người comment

        public string Content { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        // Danh sách reply con (cấp 1). Mỗi phần tử cũng kiểu ResponseCommentDto.
        public List<ResponseCommentDto> Replies { get; set; } = new List<ResponseCommentDto>();
    }
}
