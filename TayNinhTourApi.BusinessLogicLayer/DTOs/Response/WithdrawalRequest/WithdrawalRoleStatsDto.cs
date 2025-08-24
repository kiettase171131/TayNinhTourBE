namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.WithdrawalRequest
{
    /// <summary>
    /// DTO cho th?ng kê yêu c?u rút ti?n theo role (TourCompany và SpecialtyShop)
    /// </summary>
    public class WithdrawalRoleStatsDto
    {
        /// <summary>
        /// Role c?a user (TourCompany ho?c SpecialtyShop)
        /// </summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// T?ng s? yêu c?u rút ti?n
        /// </summary>
        public int TotalRequests { get; set; }

        /// <summary>
        /// S? yêu c?u ?ang ch? duy?t
        /// </summary>
        public int PendingRequests { get; set; }

        /// <summary>
        /// S? yêu c?u ?ã ???c duy?t
        /// </summary>
        public int ApprovedRequests { get; set; }

        /// <summary>
        /// S? yêu c?u b? t? ch?i
        /// </summary>
        public int RejectedRequests { get; set; }

        /// <summary>
        /// T?ng s? ti?n ?ã yêu c?u rút
        /// </summary>
        public decimal TotalAmountRequested { get; set; }

        /// <summary>
        /// T?ng s? ti?n ?ang ch? duy?t
        /// </summary>
        public decimal PendingAmount { get; set; }

        /// <summary>
        /// T?ng s? ti?n ?ã ???c duy?t
        /// </summary>
        public decimal ApprovedAmount { get; set; }

        /// <summary>
        /// T?ng s? ti?n b? t? ch?i
        /// </summary>
        public decimal RejectedAmount { get; set; }

        /// <summary>
        /// Th?i gian t? ngày l?c
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Th?i gian ??n ngày l?c
        /// </summary>
        public DateTime? EndDate { get; set; }
    }

    /// <summary>
    /// DTO t?ng h?p th?ng kê cho c? TourCompany và SpecialtyShop
    /// </summary>
    public class WithdrawalRoleStatsSummaryDto
    {
        /// <summary>
        /// Th?ng kê cho TourCompany
        /// </summary>
        public WithdrawalRoleStatsDto TourCompanyStats { get; set; } = new();

        /// <summary>
        /// Th?ng kê cho SpecialtyShop
        /// </summary>
        public WithdrawalRoleStatsDto SpecialtyShopStats { get; set; } = new();

        /// <summary>
        /// Th?i gian t?o báo cáo
        /// </summary>
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Th?i gian t? ngày l?c
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Th?i gian ??n ngày l?c
        /// </summary>
        public DateTime? EndDate { get; set; }
    }
}