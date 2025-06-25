using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany
{
    /// <summary>
    /// DTO cho thông tin SpecialtyShop invitation trong TourDetails
    /// </summary>
    public class TourDetailsSpecialtyShopDto
    {
        /// <summary>
        /// ID của invitation
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ID của TourDetails
        /// </summary>
        public Guid TourDetailsId { get; set; }

        /// <summary>
        /// ID của SpecialtyShop
        /// </summary>
        public Guid SpecialtyShopId { get; set; }

        /// <summary>
        /// Thông tin SpecialtyShop
        /// </summary>
        public SpecialtyShopSummaryDto? SpecialtyShop { get; set; }

        /// <summary>
        /// Thời gian được mời
        /// </summary>
        public DateTime InvitedAt { get; set; }

        /// <summary>
        /// Trạng thái phản hồi
        /// </summary>
        public ShopInvitationStatus Status { get; set; }

        /// <summary>
        /// Tên trạng thái (để hiển thị)
        /// </summary>
        public string StatusText { get; set; } = string.Empty;

        /// <summary>
        /// Thời gian phản hồi
        /// </summary>
        public DateTime? RespondedAt { get; set; }

        /// <summary>
        /// Ghi chú phản hồi từ shop
        /// </summary>
        public string? ResponseNote { get; set; }

        /// <summary>
        /// Thời gian hết hạn invitation
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Ưu tiên hiển thị
        /// </summary>
        public int? Priority { get; set; }

        /// <summary>
        /// Có hết hạn chưa
        /// </summary>
        public bool IsExpired { get; set; }

        /// <summary>
        /// Số ngày còn lại để phản hồi
        /// </summary>
        public int DaysRemaining { get; set; }
    }

    /// <summary>
    /// DTO tóm tắt thông tin SpecialtyShop
    /// </summary>
    public class SpecialtyShopSummaryDto
    {
        /// <summary>
        /// ID của shop
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Tên shop
        /// </summary>
        public string ShopName { get; set; } = string.Empty;

        /// <summary>
        /// Địa chỉ shop
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// Số điện thoại
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Loại shop
        /// </summary>
        public string? ShopType { get; set; }

        /// <summary>
        /// Đánh giá trung bình
        /// </summary>
        public decimal? Rating { get; set; }

        /// <summary>
        /// Trạng thái hoạt động
        /// </summary>
        public bool IsShopActive { get; set; }

        /// <summary>
        /// Tên chủ shop
        /// </summary>
        public string? OwnerName { get; set; }

        /// <summary>
        /// Email chủ shop
        /// </summary>
        public string? OwnerEmail { get; set; }
    }
}
