using System;
using System.Collections.Generic;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Dashboard
{
    /// <summary>
    /// DTO cho th?ng kê thu nh?p c?a tour company - dành cho admin xem
    /// </summary>
    public class TourCompanyRevenueStatsDto
    {
        /// <summary>
        /// ID c?a tour company
        /// </summary>
        public Guid TourCompanyId { get; set; }

        /// <summary>
        /// ID c?a user có role Tour Company
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Tên công ty tour
        /// </summary>
        public string CompanyName { get; set; } = string.Empty;

        /// <summary>
        /// Email c?a tour company
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// S? ti?n trong ví (?ã ???c chuy?n sau 3 ngày)
        /// </summary>
        public decimal CurrentWalletBalance { get; set; }

        /// <summary>
        /// T?ng s? ti?n ?ang hold (ch?a chuy?n vào ví)
        /// </summary>
        public decimal TotalRevenueHold { get; set; }

        /// <summary>
        /// T?ng doanh thu tr??c thu? trong tháng (100% thanh toán t? khách)
        /// </summary>
        public decimal MonthlyRevenueBeforeTax { get; set; }

        /// <summary>
        /// T?ng doanh thu sau thu? trong tháng (80% sau khi tr? 10% hoa h?ng + 10% VAT)
        /// </summary>
        public decimal MonthlyRevenueAfterTax { get; set; }

        /// <summary>
        /// T?ng s? booking ?ã confirmed trong tháng
        /// </summary>
        public int MonthlyConfirmedBookings { get; set; }

        /// <summary>
        /// T?ng s? booking ?ang pending transfer (?? ?i?u ki?n chuy?n ti?n)
        /// </summary>
        public int PendingTransferBookings { get; set; }

        /// <summary>
        /// S? ti?n ?ang ch? transfer (t? bookings ?? ?i?u ki?n)
        /// </summary>
        public decimal PendingTransferAmount { get; set; }

        /// <summary>
        /// T?ng s? tour slots ?ã ???c ??t trong tháng
        /// </summary>
        public int MonthlyBookedSlots { get; set; }

        /// <summary>
        /// Th?ng kê tours c?a company
        /// </summary>
        public TourStatsDto TourStats { get; set; } = new();

        /// <summary>
        /// Danh sách thu nh?p t?ng tour riêng
        /// </summary>
        public List<TourRevenueDetailDto> TourRevenueDetails { get; set; } = new();

        /// <summary>
        /// Danh sách các booking trong tháng v?i thông tin chi ti?t
        /// </summary>
        public List<TourBookingDetailDto> MonthlyBookings { get; set; } = new();

        /// <summary>
        /// Tr?ng thái ho?t ??ng c?a company
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Ngày t?o company
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// L?n c?p nh?t cu?i
        /// </summary>
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// DTO th?ng kê tours c?a company
    /// </summary>
    public class TourStatsDto
    {
        /// <summary>
        /// T?ng s? tours ?ã t?o
        /// </summary>
        public int TotalToursCreated { get; set; }

        /// <summary>
        /// S? tours ?ã ???c duy?t (Approved)
        /// </summary>
        public int ToursApproved { get; set; }

        /// <summary>
        /// S? tours ?ã public (có th? booking)
        /// </summary>
        public int ToursPublic { get; set; }

        /// <summary>
        /// S? tours ?ang ch? duy?t
        /// </summary>
        public int ToursPending { get; set; }

        /// <summary>
        /// S? tours b? t? ch?i
        /// </summary>
        public int ToursRejected { get; set; }

        /// <summary>
        /// S? tours có revenue (có booking trong tháng)
        /// </summary>
        public int ToursWithRevenue { get; set; }
    }

    /// <summary>
    /// DTO thu nh?p t?ng tour riêng
    /// </summary>
    public class TourRevenueDetailDto
    {
        /// <summary>
        /// ID c?a tour details
        /// </summary>
        public Guid TourDetailsId { get; set; }

        /// <summary>
        /// Tên tour
        /// </summary>
        public string TourTitle { get; set; } = string.Empty;

        /// <summary>
        /// Tr?ng thái tour
        /// </summary>
        public string TourStatus { get; set; } = string.Empty;

        /// <summary>
        /// Ngày t?o tour
        /// </summary>
        public DateTime TourCreatedAt { get; set; }

        /// <summary>
        /// Tour có public không (có th? booking)
        /// </summary>
        public bool IsPublic { get; set; }

        /// <summary>
        /// T?ng s? booking c?a tour trong tháng
        /// </summary>
        public int MonthlyBookingCount { get; set; }

        /// <summary>
        /// T?ng doanh thu tr??c thu? c?a tour trong tháng
        /// </summary>
        public decimal MonthlyRevenueBeforeTax { get; set; }

        /// <summary>
        /// T?ng doanh thu sau thu? c?a tour trong tháng
        /// </summary>
        public decimal MonthlyRevenueAfterTax { get; set; }

        /// <summary>
        /// S? ti?n ?ang hold c?a tour
        /// </summary>
        public decimal RevenueHold { get; set; }

        /// <summary>
        /// S? ti?n ?ã transfer vào ví
        /// </summary>
        public decimal RevenueTransferred { get; set; }

        /// <summary>
        /// S? booking có revenue ?ã ???c transfer v? wallet
        /// </summary>
        public int TransferredBookingCount { get; set; }

        /// <summary>
        /// S? booking có revenue ?ang hold (ch?a transfer)
        /// </summary>
        public int PendingTransferBookingCount { get; set; }

        /// <summary>
        /// S? booking ?? ?i?u ki?n transfer (tour ?ã hoàn thành >= 3 ngày)
        /// </summary>
        public int EligibleForTransferCount { get; set; }

        /// <summary>
        /// T?ng s? khách ?ã book tour trong tháng
        /// </summary>
        public int TotalGuestsCount { get; set; }

        /// <summary>
        /// Giá tour hi?n t?i
        /// </summary>
        public decimal? CurrentPrice { get; set; }

        /// <summary>
        /// S? slot có s?n
        /// </summary>
        public int AvailableSlots { get; set; }

        /// <summary>
        /// Thông tin chi ti?t v? revenue transfer
        /// </summary>
        public TourRevenueTransferInfoDto TransferInfo { get; set; } = new();
    }

    /// <summary>
    /// DTO thông tin chi ti?t v? revenue transfer c?a tour
    /// </summary>
    public class TourRevenueTransferInfoDto
    {
        /// <summary>
        /// Ngày transfer s?m nh?t (t? booking ??u tiên)
        /// </summary>
        public DateTime? EarliestTransferDate { get; set; }

        /// <summary>
        /// Ngày transfer mu?n nh?t (t? booking cu?i cùng)
        /// </summary>
        public DateTime? LatestTransferDate { get; set; }

        /// <summary>
        /// S? ngày trung bình t? tour completion ??n transfer
        /// </summary>
        public double AverageTransferDays { get; set; }

        /// <summary>
        /// T? l? booking ?ã ???c transfer (%)
        /// </summary>
        public double TransferCompletionRate { get; set; }

        /// <summary>
        /// Danh sách booking v?i thông tin transfer chi ti?t
        /// </summary>
        public List<BookingTransferDetailDto> BookingTransfers { get; set; } = new();
    }

    /// <summary>
    /// DTO chi ti?t transfer c?a t?ng booking
    /// </summary>
    public class BookingTransferDetailDto
    {
        /// <summary>
        /// ID booking
        /// </summary>
        public Guid BookingId { get; set; }

        /// <summary>
        /// Mã booking
        /// </summary>
        public string BookingCode { get; set; } = string.Empty;

        /// <summary>
        /// Ngày tour di?n ra
        /// </summary>
        public DateOnly TourDate { get; set; }

        /// <summary>
        /// Ngày tour completion (tour date + 1 day)
        /// </summary>
        public DateTime TourCompletionDate { get; set; }

        /// <summary>
        /// Ngày ?? ?i?u ki?n transfer (tour completion + 3 days)
        /// </summary>
        public DateTime EligibleTransferDate { get; set; }

        /// <summary>
        /// Ngày th?c t? transfer (n?u ?ã transfer)
        /// </summary>
        public DateTime? ActualTransferDate { get; set; }

        /// <summary>
        /// ?ã ???c transfer ch?a
        /// </summary>
        public bool IsTransferred { get; set; }

        /// <summary>
        /// ?? ?i?u ki?n transfer ch?a (>= 3 ngày t? tour completion)
        /// </summary>
        public bool IsEligibleForTransfer { get; set; }

        /// <summary>
        /// S? ngày t? tour completion ??n hi?n t?i/transfer date
        /// </summary>
        public int DaysSinceTourCompletion { get; set; }

        /// <summary>
        /// S? ti?n revenue c?a booking này
        /// </summary>
        public decimal RevenueAmount { get; set; }

        /// <summary>
        /// Tr?ng thái transfer
        /// </summary>
        public string TransferStatus { get; set; } = string.Empty; // "Transferred", "Eligible", "Pending", "NotEligible"
    }

    /// <summary>
    /// DTO chi ti?t booking và doanh thu (cho monthly bookings)
    /// </summary>
    public class TourBookingDetailDto
    {
        /// <summary>
        /// ID c?a booking
        /// </summary>
        public Guid BookingId { get; set; }

        /// <summary>
        /// Mã booking
        /// </summary>
        public string BookingCode { get; set; } = string.Empty;

        /// <summary>
        /// Tên tour
        /// </summary>
        public string TourTitle { get; set; } = string.Empty;

        /// <summary>
        /// Ngày tour
        /// </summary>
        public DateOnly TourDate { get; set; }

        /// <summary>
        /// S? khách
        /// </summary>
        public int GuestCount { get; set; }

        /// <summary>
        /// T?ng ti?n thanh toán (100%)
        /// </summary>
        public decimal TotalPayment { get; set; }

        /// <summary>
        /// S? ti?n ?ang trong revenue hold
        /// </summary>
        public decimal RevenueHold { get; set; }

        /// <summary>
        /// ?ã chuy?n vào ví ch?a
        /// </summary>
        public bool IsTransferredToWallet { get; set; }

        /// <summary>
        /// Ngày chuy?n vào ví (n?u có)
        /// </summary>
        public DateTime? RevenueTransferredDate { get; set; }

        /// <summary>
        /// Tr?ng thái booking
        /// </summary>
        public string BookingStatus { get; set; } = string.Empty;

        /// <summary>
        /// Ngày t?o booking
        /// </summary>
        public DateTime BookingCreatedAt { get; set; }
    }

    /// <summary>
    /// DTO t?ng h?p th?ng kê cho t?t c? tour companies
    /// </summary>
    public class AdminTourCompanyRevenueOverviewDto
    {
        /// <summary>
        /// Danh sách th?ng kê t?ng tour company
        /// </summary>
        public List<TourCompanyRevenueStatsDto> TourCompanies { get; set; } = new();

        /// <summary>
        /// T?ng s? tour companies
        /// </summary>
        public int TotalTourCompanies { get; set; }

        /// <summary>
        /// T?ng s? tour companies ho?t ??ng
        /// </summary>
        public int ActiveTourCompanies { get; set; }

        /// <summary>
        /// T?ng doanh thu h? th?ng tr??c thu? trong tháng
        /// </summary>
        public decimal TotalSystemRevenueBeforeTax { get; set; }

        /// <summary>
        /// T?ng doanh thu h? th?ng sau thu? trong tháng (cho companies)
        /// </summary>
        public decimal TotalSystemRevenueAfterTax { get; set; }

        /// <summary>
        /// T?ng hoa h?ng h? th?ng thu ???c (10%)
        /// </summary>
        public decimal TotalPlatformCommission { get; set; }

        /// <summary>
        /// T?ng VAT thu ???c (10%)
        /// </summary>
        public decimal TotalVAT { get; set; }

        /// <summary>
        /// T?ng s? bookings confirmed trong tháng
        /// </summary>
        public int TotalMonthlyBookings { get; set; }

        /// <summary>
        /// T?ng s? ti?n ?ang trong revenue hold
        /// </summary>
        public decimal TotalRevenueHold { get; set; }

        /// <summary>
        /// T?ng s? ti?n trong wallets c?a các companies
        /// </summary>
        public decimal TotalWalletBalance { get; set; }

        /// <summary>
        /// Tháng th?ng kê
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// N?m th?ng kê
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Th?i gian t?o báo cáo
        /// </summary>
        public DateTime GeneratedAt { get; set; }

        /// <summary>
        /// Thông tin phân trang
        /// </summary>
        public PaginationInfo Pagination { get; set; } = new();
    }

    /// <summary>
    /// Thông tin phân trang
    /// </summary>
    public class PaginationInfo
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasNext { get; set; }
        public bool HasPrevious { get; set; }
    }
}