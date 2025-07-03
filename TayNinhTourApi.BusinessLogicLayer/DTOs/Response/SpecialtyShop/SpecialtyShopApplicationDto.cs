using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.SpecialtyShop
{
    /// <summary>
    /// DTO response đầy đủ cho SpecialtyShopApplication
    /// Dùng cho admin xem chi tiết đơn đăng ký
    /// </summary>
    public class SpecialtyShopApplicationDto
    {
        /// <summary>
        /// ID của đơn đăng ký
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Tên specialty shop
        /// </summary>
        public string ShopName { get; set; } = null!;

        /// <summary>
        /// Mô tả specialty shop
        /// </summary>
        public string? ShopDescription { get; set; }

        /// <summary>
        /// Số giấy phép kinh doanh
        /// </summary>
        public string BusinessLicense { get; set; } = null!;

        /// <summary>
        /// Địa chỉ specialty shop
        /// </summary>
        public string Location { get; set; } = null!;

        /// <summary>
        /// Số điện thoại liên hệ
        /// </summary>
        public string PhoneNumber { get; set; } = null!;

        /// <summary>
        /// Email liên hệ
        /// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// Website specialty shop
        /// </summary>
        public string? Website { get; set; }

        /// <summary>
        /// Loại shop
        /// </summary>
        public string? ShopType { get; set; }

        /// <summary>
        /// Giờ mở cửa (HH:mm format)
        /// </summary>
        public string? OpeningHours { get; set; }

        /// <summary>
        /// Giờ đóng cửa (HH:mm format)
        /// </summary>
        public string? ClosingHours { get; set; }

        /// <summary>
        /// Tên người đại diện
        /// </summary>
        public string RepresentativeName { get; set; } = null!;

        /// <summary>
        /// URL file giấy phép kinh doanh
        /// </summary>
        public string? BusinessLicenseUrl { get; set; }

        /// <summary>
        /// URL logo specialty shop
        /// </summary>
        public string? LogoUrl { get; set; }

        /// <summary>
        /// Trạng thái đơn đăng ký
        /// </summary>
        public SpecialtyShopApplicationStatus Status { get; set; }

        /// <summary>
        /// Lý do từ chối (nếu có)
        /// </summary>
        public string? RejectionReason { get; set; }

        /// <summary>
        /// Thời gian nộp đơn
        /// </summary>
        public DateTime SubmittedAt { get; set; }

        /// <summary>
        /// Thời gian xử lý đơn
        /// </summary>
        public DateTime? ProcessedAt { get; set; }

        /// <summary>
        /// Thông tin user đăng ký
        /// </summary>
        public UserSummaryDto UserInfo { get; set; } = null!;

        /// <summary>
        /// Thông tin admin xử lý (nếu có)
        /// </summary>
        public UserSummaryDto? ProcessedByInfo { get; set; }
    }

    /// <summary>
    /// DTO tóm tắt thông tin User
    /// </summary>
    public class UserSummaryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
    }
}
