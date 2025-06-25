namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany
{
    /// <summary>
    /// DTO cho thông tin readiness của TourDetails để tạo TourOperation
    /// </summary>
    public class TourDetailsReadinessDto
    {
        /// <summary>
        /// ID của TourDetails
        /// </summary>
        public Guid TourDetailsId { get; set; }

        /// <summary>
        /// TourDetails có sẵn sàng để tạo TourOperation không
        /// </summary>
        public bool IsReady { get; set; }

        /// <summary>
        /// Có TourGuide được assign không
        /// </summary>
        public bool HasTourGuide { get; set; }

        /// <summary>
        /// Có SpecialtyShop tham gia không
        /// </summary>
        public bool HasSpecialtyShop { get; set; }

        /// <summary>
        /// Số lượng TourGuide invitations đã được accept
        /// </summary>
        public int AcceptedGuideInvitations { get; set; }

        /// <summary>
        /// Số lượng SpecialtyShop invitations đã được accept
        /// </summary>
        public int AcceptedShopInvitations { get; set; }

        /// <summary>
        /// Danh sách lý do tại sao chưa ready (nếu có)
        /// </summary>
        public List<string> MissingRequirements { get; set; } = new List<string>();

        /// <summary>
        /// Message tổng hợp về trạng thái readiness
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Thông tin chi tiết về TourGuide assignments
        /// </summary>
        public TourGuideReadinessInfo GuideInfo { get; set; } = new TourGuideReadinessInfo();

        /// <summary>
        /// Thông tin chi tiết về SpecialtyShop participations
        /// </summary>
        public SpecialtyShopReadinessInfo ShopInfo { get; set; } = new SpecialtyShopReadinessInfo();
    }

    /// <summary>
    /// Thông tin chi tiết về TourGuide readiness
    /// </summary>
    public class TourGuideReadinessInfo
    {
        /// <summary>
        /// Có guide được assign trực tiếp trong TourOperation không
        /// </summary>
        public bool HasDirectAssignment { get; set; }

        /// <summary>
        /// ID của guide được assign trực tiếp (nếu có)
        /// </summary>
        public Guid? DirectlyAssignedGuideId { get; set; }

        /// <summary>
        /// Tên của guide được assign trực tiếp (nếu có)
        /// </summary>
        public string? DirectlyAssignedGuideName { get; set; }

        /// <summary>
        /// Số lượng invitations đang pending
        /// </summary>
        public int PendingInvitations { get; set; }

        /// <summary>
        /// Số lượng invitations đã được accept
        /// </summary>
        public int AcceptedInvitations { get; set; }

        /// <summary>
        /// Số lượng invitations đã bị reject
        /// </summary>
        public int RejectedInvitations { get; set; }

        /// <summary>
        /// Danh sách guides đã accept invitation
        /// </summary>
        public List<AcceptedGuideInfo> AcceptedGuides { get; set; } = new List<AcceptedGuideInfo>();
    }

    /// <summary>
    /// Thông tin chi tiết về SpecialtyShop readiness
    /// </summary>
    public class SpecialtyShopReadinessInfo
    {
        /// <summary>
        /// Số lượng shop invitations đang pending
        /// </summary>
        public int PendingInvitations { get; set; }

        /// <summary>
        /// Số lượng shop invitations đã được accept
        /// </summary>
        public int AcceptedInvitations { get; set; }

        /// <summary>
        /// Số lượng shop invitations đã bị decline
        /// </summary>
        public int DeclinedInvitations { get; set; }

        /// <summary>
        /// Danh sách shops đã accept invitation
        /// </summary>
        public List<AcceptedShopInfo> AcceptedShops { get; set; } = new List<AcceptedShopInfo>();
    }

    /// <summary>
    /// Thông tin guide đã accept invitation
    /// </summary>
    public class AcceptedGuideInfo
    {
        /// <summary>
        /// ID của guide
        /// </summary>
        public Guid GuideId { get; set; }

        /// <summary>
        /// Tên guide
        /// </summary>
        public string GuideName { get; set; } = string.Empty;

        /// <summary>
        /// Email guide
        /// </summary>
        public string GuideEmail { get; set; } = string.Empty;

        /// <summary>
        /// Thời gian accept invitation
        /// </summary>
        public DateTime AcceptedAt { get; set; }
    }

    /// <summary>
    /// Thông tin shop đã accept invitation
    /// </summary>
    public class AcceptedShopInfo
    {
        /// <summary>
        /// ID của shop
        /// </summary>
        public Guid ShopId { get; set; }

        /// <summary>
        /// Tên shop
        /// </summary>
        public string ShopName { get; set; } = string.Empty;

        /// <summary>
        /// Địa chỉ shop
        /// </summary>
        public string? ShopAddress { get; set; }

        /// <summary>
        /// Thời gian accept invitation
        /// </summary>
        public DateTime AcceptedAt { get; set; }
    }
}
