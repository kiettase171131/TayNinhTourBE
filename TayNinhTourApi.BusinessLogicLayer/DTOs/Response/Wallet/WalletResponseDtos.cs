namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Wallet
{
    /// <summary>
    /// Response DTO cho th�ng tin v� ti?n c?a TourCompany
    /// </summary>
    public class TourCompanyWalletDto
    {
        /// <summary>
        /// ID c?a TourCompany
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ID c?a User s? h?u
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// T�n c�ng ty
        /// </summary>
        public string CompanyName { get; set; } = string.Empty;

        /// <summary>
        /// S? ti?n c� th? r�t (?� ???c chuy?n t? revenue hold sau 3 ng�y)
        /// </summary>
        public decimal Wallet { get; set; }

        /// <summary>
        /// S? ti?n ?ang hold (ch?a th? r�t, ch? tour ho�n th�nh + 3 ng�y)
        /// </summary>
        public decimal RevenueHold { get; set; }

        /// <summary>
        /// T?ng s? ti?n (Wallet + RevenueHold)
        /// </summary>
        public decimal TotalBalance => Wallet + RevenueHold;

        /// <summary>
        /// Th?i gian c?p nh?t cu?i c�ng
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Response DTO cho th�ng tin v� ti?n c?a SpecialtyShop
    /// </summary>
    public class SpecialtyShopWalletDto
    {
        /// <summary>
        /// ID c?a SpecialtyShop
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ID c?a User s? h?u
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// T�n shop
        /// </summary>
        public string ShopName { get; set; } = string.Empty;

        /// <summary>
        /// S? ti?n trong v� t? vi?c b�n s?n ph?m
        /// </summary>
        public decimal Wallet { get; set; }

        /// <summary>
        /// Th?i gian c?p nh?t cu?i c�ng
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Response DTO t?ng qu�t cho wallet (c� th? d�ng cho c? 2 role)
    /// </summary>
    public class WalletInfoDto
    {
        /// <summary>
        /// Lo?i v� (TourCompany ho?c SpecialtyShop)
        /// </summary>
        public string WalletType { get; set; } = string.Empty;

        /// <summary>
        /// T�n ch? s? h?u (CompanyName ho?c ShopName)
        /// </summary>
        public string OwnerName { get; set; } = string.Empty;

        /// <summary>
        /// S? ti?n c� th? s? d?ng ngay
        /// </summary>
        public decimal AvailableBalance { get; set; }

        /// <summary>
        /// S? ti?n ?ang hold (ch? c� ? TourCompany)
        /// </summary>
        public decimal? HoldBalance { get; set; }

        /// <summary>
        /// T?ng s? ti?n
        /// </summary>
        public decimal TotalBalance { get; set; }

        /// <summary>
        /// Th?i gian c?p nh?t cu?i c�ng
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}

namespace TayNinhTourApi.BusinessLogicLayer.DTOs
{
    /// <summary>
    /// DTO cho th�ng tin t�i ch�nh c?a TourCompany (b? sung cho TourRevenueService)
    /// </summary>
    public class TourCompanyFinancialInfo
    {
        /// <summary>
        /// ID c?a TourCompany
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ID c?a User c� role Tour Company
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// T�n c�ng ty tour
        /// </summary>
        public string CompanyName { get; set; } = string.Empty;

        /// <summary>
        /// S? ti?n c� th? r�t
        /// </summary>
        public decimal Wallet { get; set; }

        /// <summary>
        /// S? ti?n ?ang hold
        /// </summary>
        public decimal RevenueHold { get; set; }

        /// <summary>
        /// Tr?ng th�i ho?t ??ng
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Ng�y t?o
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Ng�y c?p nh?t cu?i
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}