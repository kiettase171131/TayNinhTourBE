namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Wallet
{
    /// <summary>
    /// Response DTO cho thông tin ví ti?n c?a TourCompany
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
        /// Tên công ty
        /// </summary>
        public string CompanyName { get; set; } = string.Empty;

        /// <summary>
        /// S? ti?n có th? rút (?ã ???c chuy?n t? revenue hold sau 3 ngày)
        /// </summary>
        public decimal Wallet { get; set; }

        /// <summary>
        /// S? ti?n ?ang hold (ch?a th? rút, ch? tour hoàn thành + 3 ngày)
        /// </summary>
        public decimal RevenueHold { get; set; }

        /// <summary>
        /// T?ng s? ti?n (Wallet + RevenueHold)
        /// </summary>
        public decimal TotalBalance => Wallet + RevenueHold;

        /// <summary>
        /// Th?i gian c?p nh?t cu?i cùng
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Response DTO cho thông tin ví ti?n c?a SpecialtyShop
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
        /// Tên shop
        /// </summary>
        public string ShopName { get; set; } = string.Empty;

        /// <summary>
        /// S? ti?n trong ví t? vi?c bán s?n ph?m
        /// </summary>
        public decimal Wallet { get; set; }

        /// <summary>
        /// Th?i gian c?p nh?t cu?i cùng
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Response DTO t?ng quát cho wallet (có th? dùng cho c? 2 role)
    /// </summary>
    public class WalletInfoDto
    {
        /// <summary>
        /// Lo?i ví (TourCompany ho?c SpecialtyShop)
        /// </summary>
        public string WalletType { get; set; } = string.Empty;

        /// <summary>
        /// Tên ch? s? h?u (CompanyName ho?c ShopName)
        /// </summary>
        public string OwnerName { get; set; } = string.Empty;

        /// <summary>
        /// S? ti?n có th? s? d?ng ngay
        /// </summary>
        public decimal AvailableBalance { get; set; }

        /// <summary>
        /// S? ti?n ?ang hold (ch? có ? TourCompany)
        /// </summary>
        public decimal? HoldBalance { get; set; }

        /// <summary>
        /// T?ng s? ti?n
        /// </summary>
        public decimal TotalBalance { get; set; }

        /// <summary>
        /// Th?i gian c?p nh?t cu?i cùng
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}

namespace TayNinhTourApi.BusinessLogicLayer.DTOs
{
    /// <summary>
    /// DTO cho thông tin tài chính c?a TourCompany (b? sung cho TourRevenueService)
    /// </summary>
    public class TourCompanyFinancialInfo
    {
        /// <summary>
        /// ID c?a TourCompany
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ID c?a User có role Tour Company
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Tên công ty tour
        /// </summary>
        public string CompanyName { get; set; } = string.Empty;

        /// <summary>
        /// S? ti?n có th? rút
        /// </summary>
        public decimal Wallet { get; set; }

        /// <summary>
        /// S? ti?n ?ang hold
        /// </summary>
        public decimal RevenueHold { get; set; }

        /// <summary>
        /// Tr?ng thái ho?t ??ng
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Ngày t?o
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Ngày c?p nh?t cu?i
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}