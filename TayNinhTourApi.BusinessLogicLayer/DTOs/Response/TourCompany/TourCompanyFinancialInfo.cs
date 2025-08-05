namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany
{
    /// <summary>
    /// DTO cho th�ng tin t�i ch�nh c?a TourCompany
    /// ENHANCED: Updated for booking-level revenue hold system
    /// </summary>
    public class TourCompanyFinancialInfo
    {
        /// <summary>
        /// ID c?a TourCompany
        /// </summary>
        public Guid TourCompanyId { get; set; }

        /// <summary>
        /// ID c?a User c� role Tour Company
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// T�n c�ng ty tour
        /// </summary>
        public string CompanyName { get; set; } = string.Empty;

        /// <summary>
        /// S? ti?n c� th? r�t (?� ???c chuy?n t? revenue hold sau 3 ng�y)
        /// </summary>
        public decimal Wallet { get; set; }

        /// <summary>
        /// S? ti?n ?ang hold (ch?a th? r�t, ch? tour ho�n th�nh + 3 ng�y)
        /// NEW: Calculated from sum of all booking revenue holds
        /// </summary>
        public decimal RevenueHold { get; set; }

        /// <summary>
        /// T?ng s? ti?n (Wallet + RevenueHold)
        /// </summary>
        public decimal TotalRevenue { get; set; }

        /// <summary>
        /// S? l??ng bookings ?? ?i?u ki?n chuy?n ti?n (tour ho�n th�nh + 3 ng�y)
        /// NEW: Count of bookings ready for revenue transfer
        /// </summary>
        public int EligibleTransferBookings { get; set; }

        /// <summary>
        /// Tr?ng th�i ho?t ??ng c?a c�ng ty
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Ng�y c?p nh?t cu?i c�ng
        /// NEW: Last updated timestamp
        /// </summary>
        public DateTime LastUpdated { get; set; }
    }
}