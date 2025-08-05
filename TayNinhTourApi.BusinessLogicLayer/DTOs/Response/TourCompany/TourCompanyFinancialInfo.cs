namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany
{
    /// <summary>
    /// DTO cho thông tin tài chính c?a TourCompany
    /// ENHANCED: Updated for booking-level revenue hold system
    /// </summary>
    public class TourCompanyFinancialInfo
    {
        /// <summary>
        /// ID c?a TourCompany
        /// </summary>
        public Guid TourCompanyId { get; set; }

        /// <summary>
        /// ID c?a User có role Tour Company
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Tên công ty tour
        /// </summary>
        public string CompanyName { get; set; } = string.Empty;

        /// <summary>
        /// S? ti?n có th? rút (?ã ???c chuy?n t? revenue hold sau 3 ngày)
        /// </summary>
        public decimal Wallet { get; set; }

        /// <summary>
        /// S? ti?n ?ang hold (ch?a th? rút, ch? tour hoàn thành + 3 ngày)
        /// NEW: Calculated from sum of all booking revenue holds
        /// </summary>
        public decimal RevenueHold { get; set; }

        /// <summary>
        /// T?ng s? ti?n (Wallet + RevenueHold)
        /// </summary>
        public decimal TotalRevenue { get; set; }

        /// <summary>
        /// S? l??ng bookings ?? ?i?u ki?n chuy?n ti?n (tour hoàn thành + 3 ngày)
        /// NEW: Count of bookings ready for revenue transfer
        /// </summary>
        public int EligibleTransferBookings { get; set; }

        /// <summary>
        /// Tr?ng thái ho?t ??ng c?a công ty
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Ngày c?p nh?t cu?i cùng
        /// NEW: Last updated timestamp
        /// </summary>
        public DateTime LastUpdated { get; set; }
    }
}