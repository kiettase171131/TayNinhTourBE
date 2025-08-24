namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.WithdrawalRequest
{
    /// <summary>
    /// DTO cho thống kê yêu cầu rút tiền
    /// </summary>
    public class WithdrawalStatsDto
    {
        /// <summary>
        /// Tổng số yêu cầu rút tiền
        /// </summary>
        public int TotalRequests { get; set; }

        /// <summary>
        /// Số yêu cầu đang chờ xử lý
        /// </summary>
        public int PendingRequests { get; set; }

        /// <summary>
        /// Số yêu cầu đã được duyệt
        /// </summary>
        public int ApprovedRequests { get; set; }

        /// <summary>
        /// Số yêu cầu bị từ chối
        /// </summary>
        public int RejectedRequests { get; set; }

        /// <summary>
        /// Số yêu cầu bị hủy
        /// </summary>
        public int CancelledRequests { get; set; }

        /// <summary>
        /// Tổng số tiền đã yêu cầu rút
        /// </summary>
        public decimal TotalAmountRequested { get; set; }

        /// <summary>
        /// Tổng số tiền đang chờ xử lý
        /// </summary>
        public decimal PendingAmount { get; set; }

        /// <summary>
        /// Tổng số tiền đã được duyệt
        /// </summary>
        public decimal ApprovedAmount { get; set; }

        /// <summary>
        /// Tổng số tiền bị từ chối
        /// </summary>
        public decimal RejectedAmount { get; set; }

        /// <summary>
        /// Thời gian xử lý trung bình (giờ)
        /// </summary>
        public double AverageProcessingTimeHours { get; set; }

        /// <summary>
        /// Tỷ lệ duyệt (%)
        /// </summary>
        public double ApprovalRate { get; set; }

        /// <summary>
        /// Thống kê theo tháng gần nhất
        /// </summary>
        public MonthlyStats CurrentMonth { get; set; } = new();

        /// <summary>
        /// Thống kê theo tháng trước
        /// </summary>
        public MonthlyStats PreviousMonth { get; set; } = new();

        /// <summary>
        /// Yêu cầu rút tiền gần nhất
        /// </summary>
        public DateTime? LastRequestDate { get; set; }

        /// <summary>
        /// Yêu cầu được duyệt gần nhất
        /// </summary>
        public DateTime? LastApprovalDate { get; set; }

        /// <summary>
        /// Thời gian bắt đầu lọc
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Thời gian kết thúc lọc
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Thời gian tạo báo cáo
        /// </summary>
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Thống kê theo tháng
    /// </summary>
    public class MonthlyStats
    {
        /// <summary>
        /// Tháng/năm
        /// </summary>
        public string Month { get; set; } = string.Empty;

        /// <summary>
        /// Số yêu cầu trong tháng
        /// </summary>
        public int RequestCount { get; set; }

        /// <summary>
        /// Tổng số tiền yêu cầu trong tháng
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Số yêu cầu được duyệt trong tháng
        /// </summary>
        public int ApprovedCount { get; set; }

        /// <summary>
        /// Tổng số tiền được duyệt trong tháng
        /// </summary>
        public decimal ApprovedAmount { get; set; }

        /// <summary>
        /// Tỷ lệ duyệt trong tháng (%)
        /// </summary>
        public double ApprovalRate { get; set; }
    }
}
