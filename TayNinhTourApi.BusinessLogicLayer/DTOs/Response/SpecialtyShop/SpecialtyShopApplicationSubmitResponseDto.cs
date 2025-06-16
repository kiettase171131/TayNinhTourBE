namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.SpecialtyShop
{
    /// <summary>
    /// DTO response sau khi nộp đơn đăng ký Specialty Shop thành công
    /// Thay thế cho ResponseShopSubmitDto
    /// </summary>
    public class SpecialtyShopApplicationSubmitResponseDto : BaseResposeDto
    {
        /// <summary>
        /// ID của đơn đăng ký vừa tạo
        /// </summary>
        public Guid ApplicationId { get; set; }

        /// <summary>
        /// Tên specialty shop
        /// </summary>
        public string ShopName { get; set; } = null!;

        /// <summary>
        /// URL logo đã upload
        /// </summary>
        public string LogoUrl { get; set; } = string.Empty;

        /// <summary>
        /// URL business license đã upload
        /// </summary>
        public string BusinessLicenseUrl { get; set; } = string.Empty;

        /// <summary>
        /// Thời gian nộp đơn
        /// </summary>
        public DateTime SubmittedAt { get; set; }

        /// <summary>
        /// Thông báo hướng dẫn cho user
        /// </summary>
        public string Instructions { get; set; } = "Your specialty shop application has been submitted successfully. We will review your application within 3-5 business days and notify you via email.";
    }
}
