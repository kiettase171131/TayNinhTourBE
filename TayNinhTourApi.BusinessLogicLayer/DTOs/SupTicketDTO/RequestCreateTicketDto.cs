using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.SupTicketDTO
{
    public class RequestCreateTicketDto
    {
        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Nội dung là bắt buộc")]
        [StringLength(2000, ErrorMessage = "Nội dung không được vượt quá 2000 ký tự")]
        public string Content { get; set; } = null!;

        /// <summary>
        /// Danh sách file đính kèm (hỗ trợ: .png, .jpg, .jpeg, .webp, .pdf, .doc, .docx, .txt)
        /// Kích thước tối đa: 10MB mỗi file
        /// </summary>
        public List<IFormFile>? Files { get; set; }
    }
}
