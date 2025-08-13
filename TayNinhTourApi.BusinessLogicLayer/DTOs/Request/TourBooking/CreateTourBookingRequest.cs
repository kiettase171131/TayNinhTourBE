using System.ComponentModel.DataAnnotations;
using TayNinhTourApi.BusinessLogicLayer.Validations;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourBooking
{
    /// <summary>
    /// Request DTO để tạo booking tour mới
    /// </summary>
    public class CreateTourBookingRequest
    {
        /// <summary>
        /// ID của TourOperation cần booking (Optional - sẽ được tự động tìm từ TourSlot)
        /// </summary>
        public Guid? TourOperationId { get; set; }

        /// <summary>
        /// ID của TourSlot cụ thể mà khách hàng muốn booking (REQUIRED)
        /// Khách hàng phải chọn slot cụ thể để booking
        /// </summary>
        [Required(ErrorMessage = "TourSlot ID là bắt buộc - vui lòng chọn ngày tour cụ thể")]
        public Guid TourSlotId { get; set; }

        /// <summary>
        /// Số lượng khách
        /// </summary>
        [Required(ErrorMessage = "Số lượng khách là bắt buộc")]
        [Range(1, 50, ErrorMessage = "Số lượng khách phải từ 1 đến 50")]
        public int NumberOfGuests { get; set; }

        /// <summary>
        /// Số điện thoại liên hệ của người đặt tour
        /// </summary>
        [StringLength(20, ErrorMessage = "Số điện thoại không quá 20 ký tự")]
        public string? ContactPhone { get; set; }

        /// <summary>
        /// Ghi chú đặc biệt từ khách hàng
        /// </summary>
        [StringLength(500, ErrorMessage = "Ghi chú không quá 500 ký tự")]
        public string? SpecialRequests { get; set; }

        /// <summary>
        /// Danh sách thông tin từng khách hàng trong booking
        /// Số lượng phải khớp với NumberOfGuests
        /// Email của từng guest phải unique trong cùng booking
        /// </summary>
        [Required(ErrorMessage = "Thông tin khách hàng là bắt buộc")]
        [MinLength(1, ErrorMessage = "Phải có ít nhất 1 khách hàng")]
        [GuestListValidation]
        public List<GuestInfoRequest> Guests { get; set; } = new();
    }
}
