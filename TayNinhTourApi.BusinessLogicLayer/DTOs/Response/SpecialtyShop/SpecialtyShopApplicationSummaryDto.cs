using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.SpecialtyShop
{
    /// <summary>
    /// DTO tóm tắt cho danh sách SpecialtyShopApplication
    /// Dùng cho admin xem list pending applications
    /// </summary>
    public class SpecialtyShopApplicationSummaryDto
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
        /// Địa chỉ specialty shop
        /// </summary>
        public string Location { get; set; } = null!;

        /// <summary>
        /// Loại shop
        /// </summary>
        public string? ShopType { get; set; }

        /// <summary>
        /// Trạng thái đơn đăng ký
        /// </summary>
        public SpecialtyShopApplicationStatus Status { get; set; }

        /// <summary>
        /// Thời gian nộp đơn
        /// </summary>
        public DateTime SubmittedAt { get; set; }

        /// <summary>
        /// Thời gian xử lý đơn (nếu có)
        /// </summary>
        public DateTime? ProcessedAt { get; set; }

        /// <summary>
        /// Tên user đăng ký
        /// </summary>
        public string UserName { get; set; } = null!;

        /// <summary>
        /// Email user đăng ký
        /// </summary>
        public string UserEmail { get; set; } = null!;

        /// <summary>
        /// Tên người đại diện
        /// </summary>
        public string RepresentativeName { get; set; } = null!;

        /// <summary>
        /// Số điện thoại liên hệ
        /// </summary>
        public string PhoneNumber { get; set; } = null!;
    }
}
