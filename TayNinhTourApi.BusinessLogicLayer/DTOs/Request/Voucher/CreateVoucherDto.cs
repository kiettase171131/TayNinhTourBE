using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Voucher
{
    public class CreateVoucherDto
    {
        /// <summary>
        /// Tên voucher template (VD: "Black Friday Sale", "Tết 2025")
        /// </summary>
        [Required(ErrorMessage = "Tên voucher là bắt buộc")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Tên voucher phải từ 3-200 ký tự")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Số lượng mã voucher sẽ được tự động tạo ra
        /// </summary>
        [Required(ErrorMessage = "Số lượng voucher là bắt buộc")]
        [Range(1, 1000, ErrorMessage = "Số lượng voucher phải từ 1 đến 1000")]
        public int Quantity { get; set; }

        /// <summary>
        /// Số tiền giảm cố định (nếu dùng fixed amount)
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Số tiền giảm phải >= 0")]
        public decimal DiscountAmount { get; set; } = 0;

        /// <summary>
        /// Phần trăm giảm giá (nếu dùng percentage discount)
        /// </summary>
        [Range(0, 100, ErrorMessage = "Phần trăm giảm phải từ 0-100%")]
        public int? DiscountPercent { get; set; }

        /// <summary>
        /// Ngày bắt đầu có hiệu lực
        /// </summary>
        [Required(ErrorMessage = "Ngày bắt đầu là bắt buộc")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Ngày hết hạn
        /// </summary>
        [Required(ErrorMessage = "Ngày kết thúc là bắt buộc")]
        public DateTime EndDate { get; set; }
    }
}
