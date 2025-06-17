namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response
{
    /// <summary>
    /// Response DTO for TourGuide application submission
    /// </summary>
    public class TourGuideApplicationSubmitResponseDto
    {
        /// <summary>
        /// HTTP status code
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Response message
        /// </summary>
        public string Message { get; set; } = null!;

        /// <summary>
        /// ID của đơn đăng ký vừa tạo
        /// </summary>
        public Guid ApplicationId { get; set; }

        /// <summary>
        /// Họ tên ứng viên
        /// </summary>
        public string FullName { get; set; } = null!;

        /// <summary>
        /// URL đến file CV đã upload
        /// </summary>
        public string? CurriculumVitaeUrl { get; set; }

        /// <summary>
        /// Thời gian nộp đơn
        /// </summary>
        public DateTime SubmittedAt { get; set; }
    }
}
