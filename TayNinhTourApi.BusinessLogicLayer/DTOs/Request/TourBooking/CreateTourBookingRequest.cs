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
        /// Loại booking: Individual (mỗi khách có QR riêng) hoặc GroupRepresentative (1 QR cho cả nhóm)
        /// Default: Individual
        /// </summary>
        [StringLength(50)]
        public string BookingType { get; set; } = "Individual";

        /// <summary>
        /// Tên nhóm (chỉ áp dụng cho booking loại GroupRepresentative)
        /// </summary>
        [StringLength(200, ErrorMessage = "Tên nhóm không quá 200 ký tự")]
        public string? GroupName { get; set; }

        /// <summary>
        /// Mô tả nhóm (chỉ áp dụng cho booking loại GroupRepresentative)
        /// </summary>
        [StringLength(500, ErrorMessage = "Mô tả nhóm không quá 500 ký tự")]
        public string? GroupDescription { get; set; }

        /// <summary>
        /// Thông tin người đại diện nhóm (chỉ áp dụng cho booking loại GroupRepresentative)
        /// </summary>
        public GuestInfoRequest? GroupRepresentative { get; set; }

        /// <summary>
        /// Danh sách thông tin từng khách hàng trong booking
        /// Số lượng phải khớp với NumberOfGuests
        /// Email của từng guest phải unique trong cùng booking
        /// Lưu ý: Với BookingType = GroupRepresentative, field này có thể null hoặc empty
        /// </summary>
        [GuestListValidation]
        public List<GuestInfoRequest>? Guests { get; set; } = new();
    }
}
