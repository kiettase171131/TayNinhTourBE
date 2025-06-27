namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.SpecialtyShop
{
    /// <summary>
    /// DTO cho response của SpecialtyShop
    /// Chứa tất cả thông tin cần thiết để hiển thị cho client
    /// </summary>
    public class SpecialtyShopResponseDto
    {
        /// <summary>
        /// ID của SpecialtyShop
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ID của User sở hữu shop
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Tên của shop
        /// </summary>
        public string ShopName { get; set; } = null!;

        /// <summary>
        /// Mô tả chi tiết về shop
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Địa điểm/vị trí của shop
        /// </summary>
        public string Location { get; set; } = null!;

        /// <summary>
        /// Tên người đại diện shop
        /// </summary>
        public string RepresentativeName { get; set; } = null!;

        /// <summary>
        /// Email liên hệ của shop
        /// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// Số điện thoại liên hệ của shop
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Website của shop
        /// </summary>
        public string? Website { get; set; }

        /// <summary>
        /// Số giấy phép kinh doanh
        /// </summary>
        public string? BusinessLicense { get; set; }

        /// <summary>
        /// URL đến file giấy phép kinh doanh
        /// </summary>
        public string? BusinessLicenseUrl { get; set; }

        /// <summary>
        /// URL đến logo của shop
        /// </summary>
        public string? LogoUrl { get; set; }

        /// <summary>
        /// Loại shop
        /// </summary>
        public string? ShopType { get; set; }

        /// <summary>
        /// Giờ mở cửa của shop (HH:mm format)
        /// </summary>
        public string? OpeningHours { get; set; }

        /// <summary>
        /// Giờ đóng cửa của shop (HH:mm format)
        /// </summary>
        public string? ClosingHours { get; set; }

        /// <summary>
        /// Đánh giá trung bình của shop (1-5 sao)
        /// </summary>
        public decimal? Rating { get; set; }

        /// <summary>
        /// Trạng thái hoạt động của shop
        /// </summary>
        public bool IsShopActive { get; set; }

        /// <summary>
        /// Ngày tạo
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Ngày cập nhật cuối cùng
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        // User Information

        /// <summary>
        /// Tên của User sở hữu shop
        /// </summary>
        public string UserName { get; set; } = null!;

        /// <summary>
        /// Email của User sở hữu shop
        /// </summary>
        public string UserEmail { get; set; } = null!;

        /// <summary>
        /// Avatar của User sở hữu shop
        /// </summary>
        public string? UserAvatar { get; set; }

        /// <summary>
        /// Role của User
        /// </summary>
        public string UserRole { get; set; } = null!;
    }
}
