namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response
{
    /// <summary>
    /// DTO cho response danh sách invitations của TourGuide
    /// </summary>
    public class MyInvitationsResponseDto : BaseResposeDto
    {
        /// <summary>
        /// Danh sách invitations
        /// </summary>
        public List<TourGuideInvitationDto> Invitations { get; set; } = new();

        /// <summary>
        /// Thống kê tóm tắt
        /// </summary>
        public InvitationStatisticsDto Statistics { get; set; } = new();

        /// <summary>
        /// Thông tin phân trang (nếu có)
        /// </summary>
        public PaginationDto? Pagination { get; set; }
    }

    /// <summary>
    /// DTO cho response danh sách invitations của một TourDetails
    /// </summary>
    public class TourDetailsInvitationsResponseDto : BaseResposeDto
    {
        /// <summary>
        /// Thông tin TourDetails
        /// </summary>
        public TourDetailsBasicDto TourDetails { get; set; } = null!;

        /// <summary>
        /// Danh sách invitations
        /// </summary>
        public List<TourGuideInvitationDto> Invitations { get; set; } = new();

        /// <summary>
        /// Thống kê tóm tắt
        /// </summary>
        public InvitationStatisticsDto Statistics { get; set; } = new();
    }

    /// <summary>
    /// DTO cho thống kê invitations
    /// </summary>
    public class InvitationStatisticsDto
    {
        /// <summary>
        /// Tổng số invitations
        /// </summary>
        public int TotalInvitations { get; set; }

        /// <summary>
        /// Số invitations pending
        /// </summary>
        public int PendingCount { get; set; }

        /// <summary>
        /// Số invitations accepted
        /// </summary>
        public int AcceptedCount { get; set; }

        /// <summary>
        /// Số invitations rejected
        /// </summary>
        public int RejectedCount { get; set; }

        /// <summary>
        /// Số invitations expired
        /// </summary>
        public int ExpiredCount { get; set; }

        /// <summary>
        /// Tỷ lệ chấp nhận (%)
        /// </summary>
        public double AcceptanceRate => 
            TotalInvitations > 0 ? (double)AcceptedCount / TotalInvitations * 100 : 0;

        /// <summary>
        /// Tỷ lệ từ chối (%)
        /// </summary>
        public double RejectionRate => 
            TotalInvitations > 0 ? (double)RejectedCount / TotalInvitations * 100 : 0;

        /// <summary>
        /// Invitation gần nhất
        /// </summary>
        public DateTime? LatestInvitation { get; set; }

        /// <summary>
        /// Response gần nhất
        /// </summary>
        public DateTime? LatestResponse { get; set; }
    }

    /// <summary>
    /// DTO cho thông tin phân trang
    /// </summary>
    public class PaginationDto
    {
        /// <summary>
        /// Trang hiện tại (0-based)
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// Kích thước trang
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Tổng số items
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Tổng số trang
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Có trang trước không
        /// </summary>
        public bool HasPreviousPage => CurrentPage > 0;

        /// <summary>
        /// Có trang sau không
        /// </summary>
        public bool HasNextPage => CurrentPage < TotalPages - 1;
    }

    /// <summary>
    /// DTO cho response chi tiết một invitation
    /// </summary>
    public class InvitationDetailsResponseDto : BaseResposeDto
    {
        /// <summary>
        /// Thông tin chi tiết invitation
        /// </summary>
        public TourGuideInvitationDetailDto Invitation { get; set; } = null!;
    }

    /// <summary>
    /// DTO chi tiết cho một invitation (bao gồm thêm thông tin)
    /// </summary>
    public class TourGuideInvitationDetailDto : TourGuideInvitationDto
    {
        /// <summary>
        /// Thông tin đầy đủ về TourDetails
        /// </summary>
        public new TourDetailsFullDto TourDetails { get; set; } = null!;

        /// <summary>
        /// Lịch sử các lần mời trước đó (nếu có)
        /// </summary>
        public List<InvitationHistoryDto> PreviousInvitations { get; set; } = new();

        /// <summary>
        /// Thông tin về skills matching
        /// </summary>
        public SkillsMatchingDto? SkillsMatching { get; set; }
    }

    /// <summary>
    /// DTO đầy đủ cho TourDetails
    /// </summary>
    public class TourDetailsFullDto : TourDetailsBasicDto
    {
        /// <summary>
        /// Thông tin TourTemplate
        /// </summary>
        public TourTemplateBasicDto TourTemplate { get; set; } = null!;

        /// <summary>
        /// Thông tin TourCompany
        /// </summary>
        public UserBasicDto CreatedBy { get; set; } = null!;

        /// <summary>
        /// Số lượng timeline items
        /// </summary>
        public int TimelineItemsCount { get; set; }
    }

    /// <summary>
    /// DTO cơ bản cho TourTemplate
    /// </summary>
    public class TourTemplateBasicDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string TemplateType { get; set; } = string.Empty;
        public string ScheduleDay { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO cho lịch sử invitation
    /// </summary>
    public class InvitationHistoryDto
    {
        public Guid Id { get; set; }
        public string InvitationType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime InvitedAt { get; set; }
        public DateTime? RespondedAt { get; set; }
        public string? RejectionReason { get; set; }
    }

    /// <summary>
    /// DTO cho thông tin skills matching
    /// </summary>
    public class SkillsMatchingDto
    {
        /// <summary>
        /// Skills yêu cầu
        /// </summary>
        public List<string> RequiredSkills { get; set; } = new();

        /// <summary>
        /// Skills của guide
        /// </summary>
        public List<string> GuideSkills { get; set; } = new();

        /// <summary>
        /// Skills match
        /// </summary>
        public List<string> MatchedSkills { get; set; } = new();

        /// <summary>
        /// Điểm matching (0-1)
        /// </summary>
        public double MatchScore { get; set; }

        /// <summary>
        /// Có phù hợp không
        /// </summary>
        public bool IsMatch { get; set; }
    }
}
