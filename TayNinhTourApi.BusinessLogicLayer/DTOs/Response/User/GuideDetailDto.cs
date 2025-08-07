namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.User
{
    /// <summary>
    /// DTO cho thông tin chi ti?t h??ng d?n viên
    /// S? d?ng cho xem detail page c?a tour guide
    /// </summary>
    public class GuideDetailDto
    {
        /// <summary>
        /// ID c?a h??ng d?n viên
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ID c?a User account liên k?t
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// ID c?a ??n ??ng ký g?c
        /// </summary>
        public Guid ApplicationId { get; set; }

        /// <summary>
        /// H? tên ??y ??
        /// </summary>
        public string FullName { get; set; } = null!;

        /// <summary>
        /// Email liên h?
        /// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// S? ?i?n tho?i
        /// </summary>
        public string PhoneNumber { get; set; } = null!;

        /// <summary>
        /// Mô t? kinh nghi?m chi ti?t
        /// </summary>
        public string Experience { get; set; } = null!;

        /// <summary>
        /// K? n?ng/chuyên môn
        /// </summary>
        public string? Skills { get; set; }

        /// <summary>
        /// Rating trung bình t? khách hàng
        /// </summary>
        public decimal Rating { get; set; }

        /// <summary>
        /// T?ng s? tour ?ã d?n
        /// </summary>
        public int TotalToursGuided { get; set; }

        /// <summary>
        /// Có available cho assignment không
        /// </summary>
        public bool IsAvailable { get; set; }

        /// <summary>
        /// Tr?ng thái ho?t ??ng
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Ghi chú b? sung v? h??ng d?n viên
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// URL ?nh ??i di?n
        /// </summary>
        public string? ProfileImageUrl { get; set; }

        /// <summary>
        /// Ngày ???c duy?t và tr? thành h??ng d?n viên
        /// </summary>
        public DateTime ApprovedAt { get; set; }

        /// <summary>
        /// ID c?a admin ?ã duy?t
        /// </summary>
        public Guid ApprovedById { get; set; }

        /// <summary>
        /// Tên admin ?ã duy?t
        /// </summary>
        public string? ApprovedByName { get; set; }

        /// <summary>
        /// Ngày t?o record
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Ngày c?p nh?t g?n nh?t
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Th?ng kê b? sung (n?u có)
        /// </summary>
        public GuideStatisticsDto? Statistics { get; set; }

        /// <summary>
        /// Thông tin User account
        /// </summary>
        public GuideUserInfoDto? UserInfo { get; set; }
    }

    /// <summary>
    /// DTO cho th?ng kê h??ng d?n viên
    /// </summary>
    public class GuideStatisticsDto
    {
        /// <summary>
        /// S? l?i m?i ?ang ch? x? lý
        /// </summary>
        public int ActiveInvitations { get; set; }

        /// <summary>
        /// S? tour ?ã hoàn thành
        /// </summary>
        public int CompletedTours { get; set; }

        /// <summary>
        /// Ngày tour g?n nh?t
        /// </summary>
        public DateTime? LastTourDate { get; set; }
    }

    /// <summary>
    /// DTO cho thông tin User c?a h??ng d?n viên
    /// </summary>
    public class GuideUserInfoDto
    {
        /// <summary>
        /// Username
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// Tên hi?n th?
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// Avatar URL
        /// </summary>
        public string? Avatar { get; set; }

        /// <summary>
        /// Ngày tham gia h? th?ng
        /// </summary>
        public DateTime JoinedDate { get; set; }

        /// <summary>
        /// Tr?ng thái User account
        /// </summary>
        public bool IsUserActive { get; set; }
    }
}