using TayNinhTourApi.DataAccessLayer.Utilities;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response
{
    /// <summary>
    /// DTO cho response trạng thái phân công hướng dẫn viên
    /// Cung cấp thông tin tổng quan về workflow assignment
    /// </summary>
    public class GuideAssignmentStatusDto
    {
        /// <summary>
        /// ID của TourDetails
        /// </summary>
        public Guid TourDetailsId { get; set; }

        /// <summary>
        /// Tiêu đề tour
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Trạng thái hiện tại của TourDetails
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Kỹ năng yêu cầu cho tour
        /// </summary>
        public string? SkillsRequired { get; set; }

        /// <summary>
        /// Thông tin hướng dẫn viên được phân công (nếu có)
        /// </summary>
        public AssignedGuideDto? AssignedGuide { get; set; }

        /// <summary>
        /// Tóm tắt thống kê invitations
        /// </summary>
        public InvitationsSummaryDto InvitationsSummary { get; set; } = new();

        /// <summary>
        /// Timeline của workflow
        /// </summary>
        public WorkflowTimelineDto Timeline { get; set; } = new();

        /// <summary>
        /// Các hành động có thể thực hiện tiếp theo
        /// </summary>
        public List<string> AvailableActions { get; set; } = new();

        /// <summary>
        /// Thời gian tạo TourDetails
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Thời gian cập nhật cuối
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO cho thông tin hướng dẫn viên được phân công
    /// </summary>
    public class AssignedGuideDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime AssignedAt { get; set; }
        public string AssignmentMethod { get; set; } = string.Empty; // "Automatic" or "Manual"
    }

    /// <summary>
    /// DTO cho tóm tắt thống kê invitations
    /// </summary>
    public class InvitationsSummaryDto
    {
        /// <summary>
        /// Tổng số lời mời đã gửi
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// Số lời mời đang chờ phản hồi
        /// </summary>
        public int Pending { get; set; }

        /// <summary>
        /// Số lời mời đã được chấp nhận
        /// </summary>
        public int Accepted { get; set; }

        /// <summary>
        /// Số lời mời đã bị từ chối
        /// </summary>
        public int Rejected { get; set; }

        /// <summary>
        /// Số lời mời đã hết hạn
        /// </summary>
        public int Expired { get; set; }

        /// <summary>
        /// Lời mời gần nhất
        /// </summary>
        public DateTime? LastInvitationSent { get; set; }

        /// <summary>
        /// Phản hồi gần nhất
        /// </summary>
        public DateTime? LastResponse { get; set; }
    }

    /// <summary>
    /// DTO cho timeline workflow
    /// </summary>
    public class WorkflowTimelineDto
    {
        /// <summary>
        /// Thời gian tạo TourDetails
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Thời gian gửi automatic invitations (nếu có)
        /// </summary>
        public DateTime? AutomaticInvitationsSent { get; set; }

        /// <summary>
        /// Thời gian chuyển sang manual selection (nếu có)
        /// </summary>
        public DateTime? TransitionedToManualSelection { get; set; }

        /// <summary>
        /// Thời gian có guide được phân công (nếu có)
        /// </summary>
        public DateTime? GuideAssigned { get; set; }

        /// <summary>
        /// Thời gian admin duyệt (nếu có)
        /// </summary>
        public DateTime? AdminApproved { get; set; }

        /// <summary>
        /// Thời gian hủy tour (nếu có)
        /// </summary>
        public DateTime? CancelledAt { get; set; }

        /// <summary>
        /// Deadline để tìm guide (5 ngày từ khi tạo)
        /// </summary>
        public DateTime GuideAssignmentDeadline { get; set; }

        /// <summary>
        /// Số ngày còn lại để tìm guide
        /// </summary>
        public int? DaysUntilDeadline =>
            GuideAssignmentDeadline > VietnamTimeZoneUtility.GetVietnamNow()
                ? (int)(GuideAssignmentDeadline - VietnamTimeZoneUtility.GetVietnamNow()).TotalDays
                : null;

        /// <summary>
        /// Có đang trong thời hạn không
        /// </summary>
        public bool IsWithinDeadline => GuideAssignmentDeadline > VietnamTimeZoneUtility.GetVietnamNow();
    }
}
