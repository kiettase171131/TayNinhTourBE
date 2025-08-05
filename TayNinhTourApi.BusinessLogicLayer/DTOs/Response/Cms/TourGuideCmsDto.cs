using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Cms
{
    public class TourGuideCmsDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Experience { get; set; } = null!;
        public string? Skills { get; set; }
        public decimal Rating { get; set; }
        public int TotalToursGuided { get; set; }
        public bool IsAvailable { get; set; }
        public string? Notes { get; set; }
        public string? ProfileImageUrl { get; set; }
        public DateTime ApprovedAt { get; set; }
        public string? UserName { get; set; } // Lấy từ navigation property User nếu muốn show
        public string? ApprovedByName { get; set; } // Lấy từ navigation property ApprovedBy nếu muốn show
    }

}
