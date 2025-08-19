using System;
using System.Collections.Generic;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Dashboard
{
    /// <summary>
    /// DTO cho th?ng k� thu nh?p c?a tour company - d�nh cho admin xem
    /// </summary>
    public class TourCompanyRevenueStatsDto
    {
        /// <summary>
        /// ID c?a tour company
        /// </summary>
        public Guid TourCompanyId { get; set; }

        /// <summary>
        /// ID c?a user c� role Tour Company
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// T�n c�ng ty tour
        /// </summary>
        public string CompanyName { get; set; } = string.Empty;

        /// <summary>
        /// Email c?a tour company
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// S? ti?n trong v� (?� ???c chuy?n sau 3 ng�y)
        /// </summary>
        public decimal CurrentWalletBalance { get; set; }

        /// <summary>
        /// T?ng s? ti?n ?ang hold (ch?a chuy?n v�o v�)
        /// </summary>
        public decimal TotalRevenueHold { get; set; }

        /// <summary>
        /// T?ng doanh thu tr??c thu? trong th�ng (100% thanh to�n t? kh�ch)
        /// </summary>
        public decimal MonthlyRevenueBeforeTax { get; set; }

        /// <summary>
        /// T?ng doanh thu sau thu? trong th�ng (80% sau khi tr? 10% hoa h?ng + 10% VAT)
        /// </summary>
        public decimal MonthlyRevenueAfterTax { get; set; }

        /// <summary>
        /// T?ng s? booking ?� confirmed trong th�ng
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
        /// T?ng s? tour slots ?� ???c ??t trong th�ng
        /// </summary>
        public int MonthlyBookedSlots { get; set; }

        /// <summary>
        /// Th?ng k� tours c?a company
        /// </summary>
        public TourStatsDto TourStats { get; set; } = new();

        /// <summary>
        /// Danh s�ch thu nh?p t?ng tour ri�ng
        /// </summary>
        public List<TourRevenueDetailDto> TourRevenueDetails { get; set; } = new();

        /// <summary>
        /// Danh s�ch c�c booking trong th�ng v?i th�ng tin chi ti?t
        /// </summary>
        public List<TourBookingDetailDto> MonthlyBookings { get; set; } = new();

        /// <summary>
        /// Tr?ng th�i ho?t ??ng c?a company
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Ng�y t?o company
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// L?n c?p nh?t cu?i
        /// </summary>
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// DTO th?ng k� tours c?a company
    /// </summary>
    public class TourStatsDto
    {
        /// <summary>
        /// T?ng s? tours ?� t?o
        /// </summary>
        public int TotalToursCreated { get; set; }

        /// <summary>
        /// S? tours ?� ???c duy?t (Approved)
        /// </summary>
        public int ToursApproved { get; set; }

        /// <summary>
        /// S? tours ?� public (c� th? booking)
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
        /// S? tours c� revenue (c� booking trong th�ng)
        /// </summary>
        public int ToursWithRevenue { get; set; }
    }

    /// <summary>
    /// DTO thu nh?p t?ng tour ri�ng
    /// </summary>
    public class TourRevenueDetailDto
    {
        /// <summary>
        /// ID c?a tour details
        /// </summary>
        public Guid TourDetailsId { get; set; }

        /// <summary>
        /// T�n tour
        /// </summary>
        public string TourTitle { get; set; } = string.Empty;

        /// <summary>
        /// Tr?ng th�i tour
        /// </summary>
        public string TourStatus { get; set; } = string.Empty;

        /// <summary>
        /// Ng�y t?o tour
        /// </summary>
        public DateTime TourCreatedAt { get; set; }

        /// <summary>
        /// Tour c� public kh�ng (c� th? booking)
        /// </summary>
        public bool IsPublic { get; set; }

        /// <summary>
        /// T?ng s? booking c?a tour trong th�ng
        /// </summary>
        public int MonthlyBookingCount { get; set; }

        /// <summary>
        /// T?ng doanh thu tr??c thu? c?a tour trong th�ng
        /// </summary>
        public decimal MonthlyRevenueBeforeTax { get; set; }

        /// <summary>
        /// T?ng doanh thu sau thu? c?a tour trong th�ng
        /// </summary>
        public decimal MonthlyRevenueAfterTax { get; set; }

        /// <summary>
        /// S? ti?n ?ang hold c?a tour
        /// </summary>
        public decimal RevenueHold { get; set; }

        /// <summary>
        /// S? ti?n ?� transfer v�o v�
        /// </summary>
        public decimal RevenueTransferred { get; set; }

        /// <summary>
        /// S? booking c� revenue ?� ???c transfer v? wallet
        /// </summary>
        public int TransferredBookingCount { get; set; }

        /// <summary>
        /// S? booking c� revenue ?ang hold (ch?a transfer)
        /// </summary>
        public int PendingTransferBookingCount { get; set; }

        /// <summary>
        /// S? booking ?? ?i?u ki?n transfer (tour ?� ho�n th�nh >= 3 ng�y)
        /// </summary>
        public int EligibleForTransferCount { get; set; }

        /// <summary>
        /// T?ng s? kh�ch ?� book tour trong th�ng
        /// </summary>
        public int TotalGuestsCount { get; set; }

        /// <summary>
        /// Gi� tour hi?n t?i
        /// </summary>
        public decimal? CurrentPrice { get; set; }

        /// <summary>
        /// S? slot c� s?n
        /// </summary>
        public int AvailableSlots { get; set; }

        /// <summary>
        /// Th�ng tin chi ti?t v? revenue transfer
        /// </summary>
        public TourRevenueTransferInfoDto TransferInfo { get; set; } = new();
    }

    /// <summary>
    /// DTO th�ng tin chi ti?t v? revenue transfer c?a tour
    /// </summary>
    public class TourRevenueTransferInfoDto
    {
        /// <summary>
        /// Ng�y transfer s?m nh?t (t? booking ??u ti�n)
        /// </summary>
        public DateTime? EarliestTransferDate { get; set; }

        /// <summary>
        /// Ng�y transfer mu?n nh?t (t? booking cu?i c�ng)
        /// </summary>
        public DateTime? LatestTransferDate { get; set; }

        /// <summary>
        /// S? ng�y trung b�nh t? tour completion ??n transfer
        /// </summary>
        public double AverageTransferDays { get; set; }

        /// <summary>
        /// T? l? booking ?� ???c transfer (%)
        /// </summary>
        public double TransferCompletionRate { get; set; }

        /// <summary>
        /// Danh s�ch booking v?i th�ng tin transfer chi ti?t
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
        /// M� booking
        /// </summary>
        public string BookingCode { get; set; } = string.Empty;

        /// <summary>
        /// Ng�y tour di?n ra
        /// </summary>
        public DateOnly TourDate { get; set; }

        /// <summary>
        /// Ng�y tour completion (tour date + 1 day)
        /// </summary>
        public DateTime TourCompletionDate { get; set; }

        /// <summary>
        /// Ng�y ?? ?i?u ki?n transfer (tour completion + 3 days)
        /// </summary>
        public DateTime EligibleTransferDate { get; set; }

        /// <summary>
        /// Ng�y th?c t? transfer (n?u ?� transfer)
        /// </summary>
        public DateTime? ActualTransferDate { get; set; }

        /// <summary>
        /// ?� ???c transfer ch?a
        /// </summary>
        public bool IsTransferred { get; set; }

        /// <summary>
        /// ?? ?i?u ki?n transfer ch?a (>= 3 ng�y t? tour completion)
        /// </summary>
        public bool IsEligibleForTransfer { get; set; }

        /// <summary>
        /// S? ng�y t? tour completion ??n hi?n t?i/transfer date
        /// </summary>
        public int DaysSinceTourCompletion { get; set; }

        /// <summary>
        /// S? ti?n revenue c?a booking n�y
        /// </summary>
        public decimal RevenueAmount { get; set; }

        /// <summary>
        /// Tr?ng th�i transfer
        /// </summary>
        public string TransferStatus { get; set; } = string.Empty; // "Transferred", "Eligible", "Pending", "NotEligible"
    }

    /// <summary>
    /// DTO chi ti?t booking v� doanh thu (cho monthly bookings)
    /// </summary>
    public class TourBookingDetailDto
    {
        /// <summary>
        /// ID c?a booking
        /// </summary>
        public Guid BookingId { get; set; }

        /// <summary>
        /// M� booking
        /// </summary>
        public string BookingCode { get; set; } = string.Empty;

        /// <summary>
        /// T�n tour
        /// </summary>
        public string TourTitle { get; set; } = string.Empty;

        /// <summary>
        /// Ng�y tour
        /// </summary>
        public DateOnly TourDate { get; set; }

        /// <summary>
        /// S? kh�ch
        /// </summary>
        public int GuestCount { get; set; }

        /// <summary>
        /// T?ng ti?n thanh to�n (100%)
        /// </summary>
        public decimal TotalPayment { get; set; }

        /// <summary>
        /// S? ti?n ?ang trong revenue hold
        /// </summary>
        public decimal RevenueHold { get; set; }

        /// <summary>
        /// ?� chuy?n v�o v� ch?a
        /// </summary>
        public bool IsTransferredToWallet { get; set; }

        /// <summary>
        /// Ng�y chuy?n v�o v� (n?u c�)
        /// </summary>
        public DateTime? RevenueTransferredDate { get; set; }

        /// <summary>
        /// Tr?ng th�i booking
        /// </summary>
        public string BookingStatus { get; set; } = string.Empty;

        /// <summary>
        /// Ng�y t?o booking
        /// </summary>
        public DateTime BookingCreatedAt { get; set; }
    }

    /// <summary>
    /// DTO t?ng h?p th?ng k� cho t?t c? tour companies
    /// </summary>
    public class AdminTourCompanyRevenueOverviewDto
    {
        /// <summary>
        /// Danh s�ch th?ng k� t?ng tour company
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
        /// T?ng doanh thu h? th?ng tr??c thu? trong th�ng
        /// </summary>
        public decimal TotalSystemRevenueBeforeTax { get; set; }

        /// <summary>
        /// T?ng doanh thu h? th?ng sau thu? trong th�ng (cho companies)
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
        /// T?ng s? bookings confirmed trong th�ng
        /// </summary>
        public int TotalMonthlyBookings { get; set; }

        /// <summary>
        /// T?ng s? ti?n ?ang trong revenue hold
        /// </summary>
        public decimal TotalRevenueHold { get; set; }

        /// <summary>
        /// T?ng s? ti?n trong wallets c?a c�c companies
        /// </summary>
        public decimal TotalWalletBalance { get; set; }

        /// <summary>
        /// Th�ng th?ng k�
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// N?m th?ng k�
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Th?i gian t?o b�o c�o
        /// </summary>
        public DateTime GeneratedAt { get; set; }

        /// <summary>
        /// Th�ng tin ph�n trang
        /// </summary>
        public PaginationInfo Pagination { get; set; } = new();
    }

    /// <summary>
    /// Th�ng tin ph�n trang
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