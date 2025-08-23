namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.WithdrawalRequest
{
    /// <summary>
    /// DTO cho th?ng k� y�u c?u r�t ti?n theo role (TourCompany v� SpecialtyShop)
    /// </summary>
    public class WithdrawalRoleStatsDto
    {
        /// <summary>
        /// Role c?a user (TourCompany ho?c SpecialtyShop)
        /// </summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// T?ng s? y�u c?u r�t ti?n
        /// </summary>
        public int TotalRequests { get; set; }

        /// <summary>
        /// S? y�u c?u ?ang ch? duy?t
        /// </summary>
        public int PendingRequests { get; set; }

        /// <summary>
        /// S? y�u c?u ?� ???c duy?t
        /// </summary>
        public int ApprovedRequests { get; set; }

        /// <summary>
        /// S? y�u c?u b? t? ch?i
        /// </summary>
        public int RejectedRequests { get; set; }

        /// <summary>
        /// T?ng s? ti?n ?� y�u c?u r�t
        /// </summary>
        public decimal TotalAmountRequested { get; set; }

        /// <summary>
        /// T?ng s? ti?n ?ang ch? duy?t
        /// </summary>
        public decimal PendingAmount { get; set; }

        /// <summary>
        /// T?ng s? ti?n ?� ???c duy?t
        /// </summary>
        public decimal ApprovedAmount { get; set; }

        /// <summary>
        /// T?ng s? ti?n b? t? ch?i
        /// </summary>
        public decimal RejectedAmount { get; set; }

        /// <summary>
        /// Th?i gian t? ng�y l?c
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Th?i gian ??n ng�y l?c
        /// </summary>
        public DateTime? EndDate { get; set; }
    }

    /// <summary>
    /// DTO t?ng h?p th?ng k� cho c? TourCompany v� SpecialtyShop
    /// </summary>
    public class WithdrawalRoleStatsSummaryDto
    {
        /// <summary>
        /// Th?ng k� cho TourCompany
        /// </summary>
        public WithdrawalRoleStatsDto TourCompanyStats { get; set; } = new();

        /// <summary>
        /// Th?ng k� cho SpecialtyShop
        /// </summary>
        public WithdrawalRoleStatsDto SpecialtyShopStats { get; set; } = new();

        /// <summary>
        /// Th?i gian t?o b�o c�o
        /// </summary>
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Th?i gian t? ng�y l?c
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Th?i gian ??n ng�y l?c
        /// </summary>
        public DateTime? EndDate { get; set; }
    }
}