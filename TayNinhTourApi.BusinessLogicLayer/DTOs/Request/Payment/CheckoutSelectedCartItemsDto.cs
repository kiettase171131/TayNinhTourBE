using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Payment
{
    public class CheckoutSelectedCartItemsDto
    {
        [Required(ErrorMessage = "Danh sách sản phẩm không được để trống")]
        [MinLength(1, ErrorMessage = "Phải chọn ít nhất 1 sản phẩm để checkout")]
        public List<Guid> CartItemIds { get; set; } = new List<Guid>();

        /// <summary>
        /// ID của voucher được chọn để áp dụng cho đơn hàng
        /// OPTIONAL - có thể để trống nếu không sử dụng voucher
        /// </summary>
        public Guid? VoucherId { get; set; }
    }
}
