namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.User
{
    /// <summary>
    /// DTO cho thông tin hướng dẫn viên
    /// Sử dụng trong dropdown selection và display
    /// </summary>
    public class GuideDto
    {
        /// <summary>
        /// ID của hướng dẫn viên
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Họ tên đầy đủ
        /// </summary>
        public string FullName { get; set; } = null!;

        /// <summary>
        /// Email liên hệ
        /// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// Số điện thoại
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Trạng thái hoạt động
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Có available cho assignment không
        /// (không bị conflict với tour khác cùng thời gian)
        /// </summary>
        public bool IsAvailable { get; set; }

        /// <summary>
        /// Mô tả kinh nghiệm chi tiết
        /// </summary>
        public string? Experience { get; set; }

        /// <summary>
        /// Chuyên môn/khu vực
        /// </summary>
        public string? Specialization { get; set; }

        /// <summary>
        /// Rating trung bình từ khách hàng
        /// </summary>
        public decimal? AverageRating { get; set; }

        /// <summary>
        /// Số tour đã dẫn
        /// </summary>
        public int CompletedTours { get; set; }

        /// <summary>
        /// Ngày tham gia
        /// </summary>
        public DateTime JoinedDate { get; set; }

        /// <summary>
        /// Trạng thái hiện tại (Available, Busy, OnLeave)
        /// </summary>
        public string CurrentStatus { get; set; } = "Available";
    }
}
