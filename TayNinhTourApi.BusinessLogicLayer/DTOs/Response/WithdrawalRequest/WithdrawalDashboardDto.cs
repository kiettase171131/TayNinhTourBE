namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.WithdrawalRequest
{
    /// <summary>
    /// DTO cho dashboard tổng quan về withdrawal system
    /// </summary>
    public class WithdrawalDashboardDto
    {
        /// <summary>
        /// Thống kê tổng quan
        /// </summary>
        public WithdrawalOverviewStats Overview { get; set; } = new();

        /// <summary>
        /// Danh sách yêu cầu cần xử lý gấp (pending > 3 ngày)
        /// </summary>
        public List<WithdrawalRequestAdminDto> UrgentRequests { get; set; } = new();

        /// <summary>
        /// Thống kê theo ngày trong tuần qua
        /// </summary>
        public List<DailyWithdrawalStats> WeeklyStats { get; set; } = new();

        /// <summary>
        /// Top 5 shop có số tiền rút nhiều nhất trong tháng
        /// </summary>
        public List<TopWithdrawalShop> TopShops { get; set; } = new();
    }

    /// <summary>
    /// Thống kê tổng quan withdrawal
    /// </summary>
    public class WithdrawalOverviewStats
    {
        /// <summary>
        /// Tổng số yêu cầu đang chờ
        /// </summary>
        public int PendingCount { get; set; }

        /// <summary>
        /// Tổng số tiền đang chờ (VNĐ)
        /// </summary>
        public decimal PendingAmount { get; set; }

        /// <summary>
        /// Số yêu cầu đã xử lý hôm nay
        /// </summary>
        public int ProcessedToday { get; set; }

        /// <summary>
        /// Số tiền đã rút hôm nay (VNĐ)
        /// </summary>
        public decimal WithdrawnToday { get; set; }

        /// <summary>
        /// Thời gian xử lý trung bình (giờ)
        /// </summary>
        public double AverageProcessingHours { get; set; }

        /// <summary>
        /// Tỷ lệ duyệt (%)
        /// </summary>
        public double ApprovalRate { get; set; }
    }

    /// <summary>
    /// Thống kê withdrawal theo ngày
    /// </summary>
    public class DailyWithdrawalStats
    {
        /// <summary>
        /// Ngày
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Số yêu cầu trong ngày
        /// </summary>
        public int RequestCount { get; set; }

        /// <summary>
        /// Tổng số tiền yêu cầu (VNĐ)
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Số yêu cầu đã duyệt
        /// </summary>
        public int ApprovedCount { get; set; }

        /// <summary>
        /// Số tiền đã duyệt (VNĐ)
        /// </summary>
        public decimal ApprovedAmount { get; set; }
    }

    /// <summary>
    /// Top shop rút tiền nhiều
    /// </summary>
    public class TopWithdrawalShop
    {
        /// <summary>
        /// ID của shop
        /// </summary>
        public Guid ShopId { get; set; }

        /// <summary>
        /// Tên shop
        /// </summary>
        public string ShopName { get; set; } = string.Empty;

        /// <summary>
        /// Tên chủ shop
        /// </summary>
        public string OwnerName { get; set; } = string.Empty;

        /// <summary>
        /// Số lần rút tiền trong tháng
        /// </summary>
        public int WithdrawalCount { get; set; }

        /// <summary>
        /// Tổng số tiền đã rút trong tháng (VNĐ)
        /// </summary>
        public decimal TotalWithdrawn { get; set; }

        /// <summary>
        /// Số dư ví hiện tại (VNĐ)
        /// </summary>
        public decimal CurrentBalance { get; set; }
    }
}
