using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourBookingRefund
{
    /// <summary>
    /// DTO cho response thông tin yêu cầu hoàn tiền tour booking (customer view)
    /// </summary>
    public class TourRefundRequestDto
    {
        /// <summary>
        /// ID của yêu cầu hoàn tiền
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ID của tour booking
        /// </summary>
        public Guid TourBookingId { get; set; }

        /// <summary>
        /// Thông tin tour booking
        /// </summary>
        public RefundTourBookingInfo TourBooking { get; set; } = null!;

        /// <summary>
        /// Loại hoàn tiền
        /// </summary>
        public TourRefundType RefundType { get; set; }

        /// <summary>
        /// Tên loại hoàn tiền (để hiển thị)
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
        /// Số tiền được duyệt hoàn (nếu có)
        /// </summary>
        public decimal? ApprovedAmount { get; set; }

        /// <summary>
        /// Phí xử lý hoàn tiền
        /// </summary>
        public decimal ProcessingFee { get; set; }

        /// <summary>
        /// Số tiền thực tế customer nhận được
        /// </summary>
        public decimal NetRefundAmount { get; set; }

        /// <summary>
        /// Trạng thái của yêu cầu
        /// </summary>
        public TourRefundStatus Status { get; set; }

        /// <summary>
        /// Tên trạng thái (để hiển thị)
        /// </summary>
        public string StatusName { get; set; } = string.Empty;

        /// <summary>
        /// Màu sắc trạng thái (để hiển thị UI)
        /// </summary>
        public string StatusColor { get; set; } = string.Empty;

        /// <summary>
        /// Thời gian tạo yêu cầu
        /// </summary>
        public DateTime RequestedAt { get; set; }

        /// <summary>
        /// Thời gian xử lý (nếu có)
        /// </summary>
        public DateTime? ProcessedAt { get; set; }

        /// <summary>
        /// Thời gian hoàn thành (nếu có)
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Tên admin xử lý (nếu có)
        /// </summary>
        public string? ProcessedByName { get; set; }

        /// <summary>
        /// Ghi chú từ customer
        /// </summary>
        public string? CustomerNotes { get; set; }

        /// <summary>
        /// Ghi chú từ admin (nếu có)
        /// </summary>
        public string? AdminNotes { get; set; }

        /// <summary>
        /// Mã tham chiếu giao dịch (nếu có)
        /// </summary>
        public string? TransactionReference { get; set; }

        /// <summary>
        /// Thông tin ngân hàng nhận tiền hoàn
        /// </summary>
        public RefundBankInfo? BankInfo { get; set; }

        /// <summary>
        /// Số ngày trước tour khi tạo yêu cầu
        /// </summary>
        public int? DaysBeforeTour { get; set; }

        /// <summary>
        /// Phần trăm hoàn tiền theo policy
        /// </summary>
        public decimal? RefundPercentage { get; set; }

        /// <summary>
        /// Có thể hủy yêu cầu không (chỉ khi status = Pending)
        /// </summary>
        public bool CanCancel { get; set; }

        /// <summary>
        /// Có thể cập nhật thông tin ngân hàng không
        /// </summary>
        public bool CanUpdateBankInfo { get; set; }

        /// <summary>
        /// Thời gian ước tính hoàn thành (nếu có)
        /// </summary>
        public DateTime? EstimatedCompletionTime { get; set; }

        /// <summary>
        /// Timeline xử lý yêu cầu
        /// </summary>
        public List<RefundTimelineItem> Timeline { get; set; } = new();
    }

    /// <summary>
    /// Thông tin tour booking trong refund request
    /// </summary>
    public class RefundTourBookingInfo
    {
        /// <summary>
        /// ID của tour booking
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Mã booking
        /// </summary>
        public string BookingCode { get; set; } = string.Empty;

        /// <summary>
        /// Tên tour
        /// </summary>
        public string TourName { get; set; } = string.Empty;

        /// <summary>
        /// Ngày bắt đầu tour
        /// </summary>
        public DateTime TourStartDate { get; set; }

        /// <summary>
        /// Ngày kết thúc tour
        /// </summary>
        public DateTime TourEndDate { get; set; }

        /// <summary>
        /// Số lượng khách
        /// </summary>
        public int NumberOfGuests { get; set; }

        /// <summary>
        /// Tổng tiền booking
        /// </summary>
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// Trạng thái booking
        /// </summary>
        public BookingStatus BookingStatus { get; set; }

        /// <summary>
        /// Tên tour company
        /// </summary>
        public string TourCompanyName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Thông tin ngân hàng trong refund request
    /// </summary>
    public class RefundBankInfo
    {
        /// <summary>
        /// Tên ngân hàng
        /// </summary>
        public string BankName { get; set; } = string.Empty;

        /// <summary>
        /// Số tài khoản đã được mask
        /// </summary>
        public string MaskedAccountNumber { get; set; } = string.Empty;

        /// <summary>
        /// Tên chủ tài khoản
        /// </summary>
        public string AccountHolderName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Timeline item cho refund request
    /// </summary>
    public class RefundTimelineItem
    {
        /// <summary>
        /// Thời gian
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Tiêu đề sự kiện
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả chi tiết
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Người thực hiện (nếu có)
        /// </summary>
        public string? PerformedBy { get; set; }

        /// <summary>
        /// Loại sự kiện
        /// </summary>
        public RefundTimelineType Type { get; set; }

        /// <summary>
        /// Icon cho sự kiện (để hiển thị UI)
        /// </summary>
        public string Icon { get; set; } = string.Empty;

        /// <summary>
        /// Màu sắc cho sự kiện (để hiển thị UI)
        /// </summary>
        public string Color { get; set; } = string.Empty;
    }

    /// <summary>
    /// Enum cho loại timeline event
    /// </summary>
    public enum RefundTimelineType
    {
        Created = 0,
        Approved = 1,
        Rejected = 2,
        Completed = 3,
        Cancelled = 4,
        BankInfoUpdated = 5,
        AmountAdjusted = 6,
        Reassigned = 7
    }
}
