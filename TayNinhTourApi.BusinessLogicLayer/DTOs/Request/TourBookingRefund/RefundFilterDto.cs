using System.ComponentModel.DataAnnotations;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourBookingRefund
{
    /// <summary>
    /// DTO cho việc filter refund requests của customer
    /// </summary>
    public class CustomerRefundFilterDto
    {
        /// <summary>
        /// Lọc theo trạng thái
        /// </summary>
        public TourRefundStatus? Status { get; set; }

        /// <summary>
        /// Lọc theo loại hoàn tiền
        /// </summary>
        public TourRefundType? RefundType { get; set; }

        /// <summary>
        /// Lọc từ ngày
        /// </summary>
        public DateTime? FromDate { get; set; }

        /// <summary>
        /// Lọc đến ngày
        /// </summary>
        public DateTime? ToDate { get; set; }

        /// <summary>
        /// Số trang
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Số trang phải >= 1")]
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Kích thước trang
        /// </summary>
        [Range(1, 100, ErrorMessage = "Kích thước trang phải từ 1-100")]
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Sắp xếp theo (RequestedAt, Amount, Status)
        /// </summary>
        public string? SortBy { get; set; }

        /// <summary>
        /// Thứ tự sắp xếp (asc, desc)
        /// </summary>
        public string? SortOrder { get; set; } = "desc";
    }

    /// <summary>
    /// DTO cho việc filter refund requests của admin
    /// </summary>
    public class AdminRefundFilterDto
    {
        /// <summary>
        /// Lọc theo trạng thái
        /// </summary>
        public TourRefundStatus? Status { get; set; }

        /// <summary>
        /// Lọc theo loại hoàn tiền
        /// </summary>
        public TourRefundType? RefundType { get; set; }

        /// <summary>
        /// Lọc từ ngày
        /// </summary>
        public DateTime? FromDate { get; set; }

        /// <summary>
        /// Lọc đến ngày
        /// </summary>
        public DateTime? ToDate { get; set; }

        /// <summary>
        /// Tìm kiếm theo từ khóa (tên customer, email, booking code, tour name)
        /// </summary>
        [StringLength(100, ErrorMessage = "Từ khóa tìm kiếm không được vượt quá 100 ký tự")]
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Lọc theo customer ID
        /// </summary>
        public Guid? CustomerId { get; set; }

        /// <summary>
        /// Lọc theo tour company ID
        /// </summary>
        public Guid? TourCompanyId { get; set; }

        /// <summary>
        /// Lọc theo admin xử lý
        /// </summary>
        public Guid? ProcessedById { get; set; }

        /// <summary>
        /// Lọc theo khoảng số tiền tối thiểu
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Số tiền tối thiểu phải >= 0")]
        public decimal? MinAmount { get; set; }

        /// <summary>
        /// Lọc theo khoảng số tiền tối đa
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Số tiền tối đa phải >= 0")]
        public decimal? MaxAmount { get; set; }

        /// <summary>
        /// Chỉ hiển thị yêu cầu quá hạn SLA
        /// </summary>
        public bool? IsOverdue { get; set; }

        /// <summary>
        /// Lọc theo độ ưu tiên
        /// </summary>
        [Range(1, 5, ErrorMessage = "Độ ưu tiên phải từ 1-5")]
        public int? Priority { get; set; }

        /// <summary>
        /// Lọc theo risk score tối thiểu
        /// </summary>
        [Range(0, 100, ErrorMessage = "Risk score phải từ 0-100")]
        public int? MinRiskScore { get; set; }

        /// <summary>
        /// Lọc theo customer tier
        /// </summary>
        public string? CustomerTier { get; set; }

        /// <summary>
        /// Chỉ hiển thị VIP customers
        /// </summary>
        public bool? IsVipCustomer { get; set; }

        /// <summary>
        /// Lọc theo tags
        /// </summary>
        public List<string>? Tags { get; set; }

        /// <summary>
        /// Số trang
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Số trang phải >= 1")]
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Kích thước trang
        /// </summary>
        [Range(1, 100, ErrorMessage = "Kích thước trang phải từ 1-100")]
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// Sắp xếp theo
        /// </summary>
        public string? SortBy { get; set; } = "RequestedAt";

        /// <summary>
        /// Thứ tự sắp xếp
        /// </summary>
        public string? SortOrder { get; set; } = "desc";

        /// <summary>
        /// Chỉ hiển thị yêu cầu được assign cho admin hiện tại
        /// </summary>
        public bool? AssignedToMe { get; set; }

        /// <summary>
        /// Chỉ hiển thị yêu cầu chưa được assign
        /// </summary>
        public bool? Unassigned { get; set; }
    }

    /// <summary>
    /// DTO cho việc filter refund statistics
    /// </summary>
    public class RefundStatisticsFilterDto
    {
        /// <summary>
        /// Từ ngày
        /// </summary>
        public DateTime? FromDate { get; set; }

        /// <summary>
        /// Đến ngày
        /// </summary>
        public DateTime? ToDate { get; set; }

        /// <summary>
        /// Lọc theo loại hoàn tiền
        /// </summary>
        public TourRefundType? RefundType { get; set; }

        /// <summary>
        /// Lọc theo tour company
        /// </summary>
        public Guid? TourCompanyId { get; set; }

        /// <summary>
        /// Lọc theo admin xử lý
        /// </summary>
        public Guid? ProcessedById { get; set; }

        /// <summary>
        /// Nhóm theo (Day, Week, Month, Year)
        /// </summary>
        public string? GroupBy { get; set; } = "Month";

        /// <summary>
        /// Bao gồm breakdown theo refund type
        /// </summary>
        public bool IncludeRefundTypeBreakdown { get; set; } = true;

        /// <summary>
        /// Bao gồm comparison với period trước
        /// </summary>
        public bool IncludePreviousPeriodComparison { get; set; } = false;
    }

    /// <summary>
    /// DTO cho việc export refund reports
    /// </summary>
    public class ExportRefundFilterDto : AdminRefundFilterDto
    {
        /// <summary>
        /// Format export (Excel, PDF, CSV)
        /// </summary>
        [Required(ErrorMessage = "Format export là bắt buộc")]
        public string ExportFormat { get; set; } = "Excel";

        /// <summary>
        /// Bao gồm customer details
        /// </summary>
        public bool IncludeCustomerDetails { get; set; } = true;

        /// <summary>
        /// Bao gồm tour details
        /// </summary>
        public bool IncludeTourDetails { get; set; } = true;

        /// <summary>
        /// Bao gồm bank information
        /// </summary>
        public bool IncludeBankInfo { get; set; } = false;

        /// <summary>
        /// Bao gồm admin notes
        /// </summary>
        public bool IncludeAdminNotes { get; set; } = true;

        /// <summary>
        /// Bao gồm timeline
        /// </summary>
        public bool IncludeTimeline { get; set; } = false;

        /// <summary>
        /// Template name (nếu có)
        /// </summary>
        public string? TemplateName { get; set; }

        /// <summary>
        /// Tên file export (không bao gồm extension)
        /// </summary>
        [StringLength(100, ErrorMessage = "Tên file không được vượt quá 100 ký tự")]
        public string? FileName { get; set; }
    }

    /// <summary>
    /// DTO cho việc check refund eligibility
    /// </summary>
    public class RefundEligibilityCheckDto
    {
        /// <summary>
        /// ID của tour booking
        /// </summary>
        [Required(ErrorMessage = "Tour booking ID là bắt buộc")]
        public Guid TourBookingId { get; set; }

        /// <summary>
        /// Ngày dự kiến hủy (để tính policy)
        /// </summary>
        public DateTime? CancellationDate { get; set; }
    }

    /// <summary>
    /// DTO response cho refund eligibility check
    /// </summary>
    public class RefundEligibilityResponseDto
    {
        /// <summary>
        /// Có đủ điều kiện hoàn tiền không
        /// </summary>
        public bool IsEligible { get; set; }

        /// <summary>
        /// Lý do không đủ điều kiện (nếu có)
        /// </summary>
        public string? IneligibilityReason { get; set; }

        /// <summary>
        /// Số tiền gốc của booking
        /// </summary>
        public decimal OriginalAmount { get; set; }

        /// <summary>
        /// Số tiền ước tính được hoàn
        /// </summary>
        public decimal EstimatedRefundAmount { get; set; }

        /// <summary>
        /// Phí xử lý ước tính
        /// </summary>
        public decimal EstimatedProcessingFee { get; set; }

        /// <summary>
        /// Số tiền thực tế customer nhận
        /// </summary>
        public decimal EstimatedNetAmount { get; set; }

        /// <summary>
        /// Phần trăm hoàn tiền theo policy
        /// </summary>
        public decimal RefundPercentage { get; set; }

        /// <summary>
        /// Số ngày trước tour
        /// </summary>
        public int DaysBeforeTour { get; set; }

        /// <summary>
        /// Policy áp dụng
        /// </summary>
        public string? ApplicablePolicy { get; set; }

        /// <summary>
        /// Thời gian deadline để hủy với policy tốt hơn
        /// </summary>
        public DateTime? NextPolicyDeadline { get; set; }

        /// <summary>
        /// Refund percentage của policy tốt hơn
        /// </summary>
        public decimal? NextPolicyRefundPercentage { get; set; }

        /// <summary>
        /// Cảnh báo cho customer
        /// </summary>
        public List<string> Warnings { get; set; } = new();

        /// <summary>
        /// Thông tin bổ sung
        /// </summary>
        public List<string> AdditionalInfo { get; set; } = new();
    }
}
