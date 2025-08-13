using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourGuide
{
    /// <summary>
    /// Request DTO để check-in cả nhóm bằng QR code nhóm
    /// </summary>
    public class CheckInGroupRequest
    {
        /// <summary>
        /// Dữ liệu QR code của nhóm được scan
        /// Chứa thông tin booking và group information
        /// </summary>
        [Required(ErrorMessage = "QR code data là bắt buộc")]
        public string QrCodeData { get; set; } = null!;

        /// <summary>
        /// ID của tour guide thực hiện check-in (optional, có thể lấy từ token)
        /// </summary>
        public Guid? TourGuideId { get; set; }

        /// <summary>
        /// Ghi chú bổ sung khi check-in (optional)
        /// </summary>
        [StringLength(500, ErrorMessage = "Ghi chú không quá 500 ký tự")]
        public string? CheckInNotes { get; set; }

        /// <summary>
        /// Danh sách ID của các khách cụ thể cần check-in
        /// Nếu null hoặc empty, sẽ check-in toàn bộ khách trong booking
        /// </summary>
        public List<Guid>? SpecificGuestIds { get; set; }

        /// <summary>
        /// Cho phép check-in một phần nhóm (không bắt buộc tất cả phải có mặt)
        /// Default: true
        /// </summary>
        public bool AllowPartialCheckIn { get; set; } = true;
    }

    /// <summary>
    /// Response DTO sau khi check-in nhóm thành công
    /// </summary>
    public class CheckInGroupResponse
    {
        /// <summary>
        /// ID của booking đã check-in
        /// </summary>
        public Guid BookingId { get; set; }

        /// <summary>
        /// Mã booking
        /// </summary>
        public string BookingCode { get; set; } = null!;

        /// <summary>
        /// Tên nhóm (nếu có)
        /// </summary>
        public string? GroupName { get; set; }

        /// <summary>
        /// Tổng số khách trong booking
        /// </summary>
        public int TotalGuests { get; set; }

        /// <summary>
        /// Số khách đã check-in thành công trong lần này
        /// </summary>
        public int CheckedInCount { get; set; }

        /// <summary>
        /// Số khách đã check-in trước đó
        /// </summary>
        public int PreviouslyCheckedInCount { get; set; }

        /// <summary>
        /// Thời gian check-in
        /// </summary>
        public DateTime CheckInTime { get; set; }

        /// <summary>
        /// Danh sách khách đã check-in
        /// </summary>
        public List<CheckedInGuestInfo> CheckedInGuests { get; set; } = new();

        /// <summary>
        /// Thông báo kết quả
        /// </summary>
        public string Message { get; set; } = null!;

        /// <summary>
        /// Trạng thái check-in có hoàn toàn thành công không
        /// </summary>
        public bool IsCompleteCheckIn { get; set; }
    }

    /// <summary>
    /// Thông tin khách đã check-in
    /// </summary>
    public class CheckedInGuestInfo
    {
        public Guid GuestId { get; set; }
        public string GuestName { get; set; } = null!;
        public string? GuestEmail { get; set; }
        public bool IsGroupRepresentative { get; set; }
        public DateTime CheckInTime { get; set; }
    }
}