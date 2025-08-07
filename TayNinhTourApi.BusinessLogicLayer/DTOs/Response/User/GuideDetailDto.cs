namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.User
{
    /// <summary>
    /// DTO cho th�ng tin chi ti?t h??ng d?n vi�n
    /// S? d?ng cho xem detail page c?a tour guide
    /// </summary>
    public class GuideDetailDto
    {
        /// <summary>
        /// ID c?a h??ng d?n vi�n
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ID c?a User account li�n k?t
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// ID c?a ??n ??ng k� g?c
        /// </summary>
        public Guid ApplicationId { get; set; }

        /// <summary>
        /// H? t�n ??y ??
        /// </summary>
        public string FullName { get; set; } = null!;

        /// <summary>
        /// Email li�n h?
        /// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// S? ?i?n tho?i
        /// </summary>
        public string PhoneNumber { get; set; } = null!;

        /// <summary>
        /// M� t? kinh nghi?m chi ti?t
        /// </summary>
        public string Experience { get; set; } = null!;

        /// <summary>
        /// K? n?ng/chuy�n m�n
        /// </summary>
        public string? Skills { get; set; }

        /// <summary>
        /// Rating trung b�nh t? kh�ch h�ng
        /// </summary>
        public decimal Rating { get; set; }

        /// <summary>
        /// T?ng s? tour ?� d?n
        /// </summary>
        public int TotalToursGuided { get; set; }

        /// <summary>
        /// C� available cho assignment kh�ng
        /// </summary>
        public bool IsAvailable { get; set; }

        /// <summary>
        /// Tr?ng th�i ho?t ??ng
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Ghi ch� b? sung v? h??ng d?n vi�n
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// URL ?nh ??i di?n
        /// </summary>
        public string? ProfileImageUrl { get; set; }

        /// <summary>
        /// Ng�y ???c duy?t v� tr? th�nh h??ng d?n vi�n
        /// </summary>
        public DateTime ApprovedAt { get; set; }

        /// <summary>
        /// ID c?a admin ?� duy?t
        /// </summary>
        public Guid ApprovedById { get; set; }

        /// <summary>
        /// T�n admin ?� duy?t
        /// </summary>
        public string? ApprovedByName { get; set; }

        /// <summary>
        /// Ng�y t?o record
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Ng�y c?p nh?t g?n nh?t
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Th?ng k� b? sung (n?u c�)
        /// </summary>
        public GuideStatisticsDto? Statistics { get; set; }

        /// <summary>
        /// Th�ng tin User account
        /// </summary>
        public GuideUserInfoDto? UserInfo { get; set; }
    }

    /// <summary>
    /// DTO cho th?ng k� h??ng d?n vi�n
    /// </summary>
    public class GuideStatisticsDto
    {
        /// <summary>
        /// S? l?i m?i ?ang ch? x? l�
        /// </summary>
        public int ActiveInvitations { get; set; }

        /// <summary>
        /// S? tour ?� ho�n th�nh
        /// </summary>
        public int CompletedTours { get; set; }

        /// <summary>
        /// Ng�y tour g?n nh?t
        /// </summary>
        public DateTime? LastTourDate { get; set; }
    }

    /// <summary>
    /// DTO cho th�ng tin User c?a h??ng d?n vi�n
    /// </summary>
    public class GuideUserInfoDto
    {
        /// <summary>
        /// Username
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// T�n hi?n th?
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// Avatar URL
        /// </summary>
        public string? Avatar { get; set; }

        /// <summary>
        /// Ng�y tham gia h? th?ng
        /// </summary>
        public DateTime JoinedDate { get; set; }

        /// <summary>
        /// Tr?ng th�i User account
        /// </summary>
        public bool IsUserActive { get; set; }
    }
}