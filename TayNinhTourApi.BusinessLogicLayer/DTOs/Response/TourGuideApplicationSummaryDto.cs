using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response
{
    /// <summary>
    /// Summary DTO for TourGuide application list view
    /// </summary>
    public class TourGuideApplicationSummaryDto
    {
        /// <summary>
        /// ID của đơn đăng ký
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Họ tên ứng viên
        /// </summary>
        public string FullName { get; set; } = null!;

        /// <summary>
        /// Số điện thoại
        /// </summary>
        public string PhoneNumber { get; set; } = null!;

        /// <summary>
        /// Số năm kinh nghiệm
        /// </summary>
        public int Experience { get; set; }

        /// <summary>
        /// Trạng thái đơn đăng ký
        /// </summary>
        public TourGuideApplicationStatus Status { get; set; }

        /// <summary>
        /// Thời gian nộp đơn
        /// </summary>
        public DateTime SubmittedAt { get; set; }

        /// <summary>
        /// Tên user đăng ký
        /// </summary>
        public string UserName { get; set; } = null!;

        /// <summary>
        /// Email user đăng ký
        /// </summary>
        public string UserEmail { get; set; } = null!;
    }
}
