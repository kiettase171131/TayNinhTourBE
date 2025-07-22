using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourBookingRefund
{
    /// <summary>
    /// DTO cho admin view của tour refund request
    /// </summary>
    public class AdminTourRefundDto
    {
        /// <summary>
        /// ID của yêu cầu hoàn tiền
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Thông tin customer
        /// </summary>
        public RefundCustomerInfo Customer { get; set; } = null!;

        /// <summary>
        /// Thông tin tour booking
        /// </summary>
        public RefundTourBookingInfo TourBooking { get; set; } = null!;

        /// <summary>
        /// Loại hoàn tiền
        /// </summary>
        public TourRefundType RefundType { get; set; }

        /// <summary>
        /// Tên loại hoàn tiền
        /// </summary>
        public string RefundTypeName { get; set; } = string.Empty;

        /// <summary>
        /// Lý do hoàn tiền
        /// </summary>
        public string RefundReason { get; set; } = string.Empty;

        /// <summary>
        /// Số tiền gốc của booking
        /// </summary>
        public decimal OriginalAmount { get; set; }

        /// <summary>
        /// Số tiền yêu cầu hoàn
        /// </summary>
        public decimal RequestedAmount { get; set; }

        /// <summary>
        /// Số tiền được duyệt hoàn
        /// </summary>
        public decimal? ApprovedAmount { get; set; }

        /// <summary>
        /// Phí xử lý
        /// </summary>
        public decimal ProcessingFee { get; set; }

        /// <summary>
        /// Số tiền thực tế customer nhận
        /// </summary>
        public decimal NetRefundAmount { get; set; }

        /// <summary>
        /// Trạng thái yêu cầu
        /// </summary>
        public TourRefundStatus Status { get; set; }

        /// <summary>
        /// Tên trạng thái
        /// </summary>
        public string StatusName { get; set; } = string.Empty;

        /// <summary>
        /// Thời gian tạo yêu cầu
        /// </summary>
        public DateTime RequestedAt { get; set; }

        /// <summary>
        /// Thời gian xử lý
        /// </summary>
        public DateTime? ProcessedAt { get; set; }

        /// <summary>
        /// Thời gian hoàn thành
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Thông tin admin xử lý
        /// </summary>
        public RefundProcessorInfo? ProcessedBy { get; set; }

        /// <summary>
        /// Ghi chú từ customer
        /// </summary>
        public string? CustomerNotes { get; set; }

        /// <summary>
        /// Ghi chú từ admin
        /// </summary>
        public string? AdminNotes { get; set; }

        /// <summary>
        /// Mã tham chiếu giao dịch
        /// </summary>
        public string? TransactionReference { get; set; }

        /// <summary>
        /// Thông tin ngân hàng customer
        /// </summary>
        public RefundBankInfo? BankInfo { get; set; }

        /// <summary>
        /// Số ngày trước tour
        /// </summary>
        public int? DaysBeforeTour { get; set; }

        /// <summary>
        /// Phần trăm hoàn tiền theo policy
        /// </summary>
        public decimal? RefundPercentage { get; set; }

        /// <summary>
        /// Độ ưu tiên xử lý (1-5, 5 = cao nhất)
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Thời gian ước tính xử lý (SLA)
        /// </summary>
        public DateTime? SlaDeadline { get; set; }

        /// <summary>
        /// Có quá hạn SLA không
        /// </summary>
        public bool IsOverdue { get; set; }

        /// <summary>
        /// Thời gian xử lý (giờ)
        /// </summary>
        public double? ProcessingTimeHours { get; set; }

        /// <summary>
        /// Có thể approve không
        /// </summary>
        public bool CanApprove { get; set; }

        /// <summary>
        /// Có thể reject không
        /// </summary>
        public bool CanReject { get; set; }

        /// <summary>
        /// Có thể reassign không
        /// </summary>
        public bool CanReassign { get; set; }

        /// <summary>
        /// Tags cho refund request
        /// </summary>
        public List<string> Tags { get; set; } = new();

        /// <summary>
        /// Risk score (0-100, 100 = rủi ro cao nhất)
        /// </summary>
        public int RiskScore { get; set; }

        /// <summary>
        /// Lý do risk score
        /// </summary>
        public string? RiskReason { get; set; }
    }

    /// <summary>
    /// Thông tin customer trong admin view
    /// </summary>
    public class RefundCustomerInfo
    {
        /// <summary>
        /// ID của customer
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Tên customer
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Email customer
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Số điện thoại
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Ngày tham gia hệ thống
        /// </summary>
        public DateTime JoinedAt { get; set; }

        /// <summary>
        /// Số lượng booking đã thực hiện
        /// </summary>
        public int TotalBookings { get; set; }

        /// <summary>
        /// Số lượng refund requests đã tạo
        /// </summary>
        public int TotalRefundRequests { get; set; }

        /// <summary>
        /// Customer tier (Bronze, Silver, Gold, Platinum)
        /// </summary>
        public string CustomerTier { get; set; } = string.Empty;

        /// <summary>
        /// Có phải VIP customer không
        /// </summary>
        public bool IsVip { get; set; }
    }

    /// <summary>
    /// Thông tin admin xử lý refund
    /// </summary>
    public class RefundProcessorInfo
    {
        /// <summary>
        /// ID của admin
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Tên admin
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Email admin
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Role của admin
        /// </summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// Số lượng refund đã xử lý trong tháng
        /// </summary>
        public int MonthlyProcessedCount { get; set; }

        /// <summary>
        /// Thời gian xử lý trung bình (giờ)
        /// </summary>
        public double AverageProcessingTime { get; set; }
    }

    /// <summary>
    /// DTO cho dashboard thống kê refund của admin
    /// </summary>
    public class AdminRefundDashboardDto
    {
        /// <summary>
        /// Tổng số yêu cầu hoàn tiền
        /// </summary>
        public int TotalRequests { get; set; }

        /// <summary>
        /// Số yêu cầu đang chờ xử lý
        /// </summary>
        public int PendingRequests { get; set; }

        /// <summary>
        /// Số yêu cầu đã approve
        /// </summary>
        public int ApprovedRequests { get; set; }

        /// <summary>
        /// Số yêu cầu đã reject
        /// </summary>
        public int RejectedRequests { get; set; }

        /// <summary>
        /// Số yêu cầu đã hoàn thành
        /// </summary>
        public int CompletedRequests { get; set; }

        /// <summary>
        /// Tổng số tiền đã hoàn
        /// </summary>
        public decimal TotalRefundedAmount { get; set; }

        /// <summary>
        /// Số tiền đang chờ hoàn
        /// </summary>
        public decimal PendingRefundAmount { get; set; }

        /// <summary>
        /// Thời gian xử lý trung bình (giờ)
        /// </summary>
        public double AverageProcessingTime { get; set; }

        /// <summary>
        /// Số yêu cầu quá hạn SLA
        /// </summary>
        public int OverdueRequests { get; set; }

        /// <summary>
        /// Thống kê theo loại hoàn tiền
        /// </summary>
        public List<RefundTypeStats> RefundTypeStats { get; set; } = new();

        /// <summary>
        /// Thống kê theo tháng (12 tháng gần nhất)
        /// </summary>
        public List<MonthlyRefundStats> MonthlyStats { get; set; } = new();

        /// <summary>
        /// Top customers có nhiều refund requests nhất
        /// </summary>
        public List<TopRefundCustomer> TopRefundCustomers { get; set; } = new();

        /// <summary>
        /// Yêu cầu cần xử lý gấp
        /// </summary>
        public List<UrgentRefundRequest> UrgentRequests { get; set; } = new();
    }

    /// <summary>
    /// Thống kê theo loại hoàn tiền
    /// </summary>
    public class RefundTypeStats
    {
        /// <summary>
        /// Loại hoàn tiền
        /// </summary>
        public TourRefundType RefundType { get; set; }

        /// <summary>
        /// Tên loại hoàn tiền
        /// </summary>
        public string RefundTypeName { get; set; } = string.Empty;

        /// <summary>
        /// Số lượng yêu cầu
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Tổng số tiền
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Phần trăm so với tổng
        /// </summary>
        public decimal Percentage { get; set; }
    }

    /// <summary>
    /// Thống kê theo tháng
    /// </summary>
    public class MonthlyRefundStats
    {
        /// <summary>
        /// Tháng/năm
        /// </summary>
        public string Month { get; set; } = string.Empty;

        /// <summary>
        /// Số lượng yêu cầu
        /// </summary>
        public int RequestCount { get; set; }

        /// <summary>
        /// Tổng số tiền hoàn
        /// </summary>
        public decimal RefundedAmount { get; set; }

        /// <summary>
        /// Thời gian xử lý trung bình
        /// </summary>
        public double AverageProcessingTime { get; set; }
    }

    /// <summary>
    /// Top customer có nhiều refund requests
    /// </summary>
    public class TopRefundCustomer
    {
        /// <summary>
        /// Thông tin customer
        /// </summary>
        public RefundCustomerInfo Customer { get; set; } = null!;

        /// <summary>
        /// Số lượng refund requests
        /// </summary>
        public int RefundRequestCount { get; set; }

        /// <summary>
        /// Tổng số tiền đã hoàn
        /// </summary>
        public decimal TotalRefundedAmount { get; set; }
    }

    /// <summary>
    /// Yêu cầu hoàn tiền cần xử lý gấp
    /// </summary>
    public class UrgentRefundRequest
    {
        /// <summary>
        /// ID yêu cầu
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Tên customer
        /// </summary>
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>
        /// Số tiền yêu cầu hoàn
        /// </summary>
        public decimal RequestedAmount { get; set; }

        /// <summary>
        /// Thời gian tạo yêu cầu
        /// </summary>
        public DateTime RequestedAt { get; set; }

        /// <summary>
        /// SLA deadline
        /// </summary>
        public DateTime SlaDeadline { get; set; }

        /// <summary>
        /// Số giờ quá hạn
        /// </summary>
        public double OverdueHours { get; set; }

        /// <summary>
        /// Độ ưu tiên
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Lý do urgent
        /// </summary>
        public string UrgentReason { get; set; } = string.Empty;
    }
}
