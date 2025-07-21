using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourBooking
{
    /// <summary>
    /// Request DTO để tạo booking tour mới
    /// </summary>
    public class CreateTourBookingRequest
    {
        /// <summary>
        /// ID của TourOperation cần booking
        /// </summary>
        [Required(ErrorMessage = "TourOperation ID là bắt buộc")]
        public Guid TourOperationId { get; set; }

        /// <summary>
        /// Số lượng khách
        /// </summary>
        [Required(ErrorMessage = "Số lượng khách là bắt buộc")]
        [Range(1, 50, ErrorMessage = "Số lượng khách phải từ 1 đến 50")]
        public int NumberOfGuests { get; set; }

        /// <summary>
        /// Số lượng người lớn
        /// </summary>
        [Required(ErrorMessage = "Số lượng người lớn là bắt buộc")]
        [Range(0, 50, ErrorMessage = "Số lượng người lớn phải từ 0 đến 50")]
        public int AdultCount { get; set; }

        /// <summary>
        /// Số lượng trẻ em
        /// </summary>
        [Range(0, 50, ErrorMessage = "Số lượng trẻ em phải từ 0 đến 50")]
        public int ChildCount { get; set; } = 0;

        /// <summary>
        /// Tên người liên hệ
        /// </summary>
        [StringLength(100, ErrorMessage = "Tên liên hệ không quá 100 ký tự")]
        public string? ContactName { get; set; }

        /// <summary>
        /// Số điện thoại liên hệ
        /// </summary>
        [StringLength(20, ErrorMessage = "Số điện thoại không quá 20 ký tự")]
        public string? ContactPhone { get; set; }

        /// <summary>
        /// Email liên hệ
        /// </summary>
        [StringLength(100, ErrorMessage = "Email không quá 100 ký tự")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? ContactEmail { get; set; }

        /// <summary>
        /// Ghi chú đặc biệt từ khách hàng
        /// </summary>
        [StringLength(500, ErrorMessage = "Ghi chú không quá 500 ký tự")]
        public string? SpecialRequests { get; set; }

        /// <summary>
        /// ID của TourSlot cụ thể mà khách hàng muốn booking (optional)
        /// Nếu có, booking sẽ được gắn với ngày cụ thể của slot này
        /// </summary>
        public Guid? TourSlotId { get; set; }
    }
}
