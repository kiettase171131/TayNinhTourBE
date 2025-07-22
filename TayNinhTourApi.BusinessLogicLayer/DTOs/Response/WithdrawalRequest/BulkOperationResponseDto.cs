namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.WithdrawalRequest
{
    /// <summary>
    /// DTO cho kết quả bulk operations (approve/reject nhiều yêu cầu cùng lúc)
    /// </summary>
    public class BulkOperationResponseDto
    {
        /// <summary>
        /// Tổng số yêu cầu được xử lý
        /// </summary>
        public int TotalRequests { get; set; }

        /// <summary>
        /// Số yêu cầu xử lý thành công
        /// </summary>
        public int SuccessCount { get; set; }

        /// <summary>
        /// Số yêu cầu xử lý thất bại
        /// </summary>
        public int FailureCount { get; set; }

        /// <summary>
        /// Tỷ lệ thành công (%)
        /// </summary>
        public double SuccessRate => TotalRequests > 0 ? (double)SuccessCount / TotalRequests * 100 : 0;

        /// <summary>
        /// Tổng số tiền được xử lý (VNĐ)
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Số tiền xử lý thành công (VNĐ)
        /// </summary>
        public decimal SuccessAmount { get; set; }

        /// <summary>
        /// Chi tiết kết quả từng yêu cầu
        /// </summary>
        public List<BulkOperationItemResult> Results { get; set; } = new();

        /// <summary>
        /// Thời gian bắt đầu xử lý
        /// </summary>
        public DateTime StartedAt { get; set; }

        /// <summary>
        /// Thời gian hoàn thành
        /// </summary>
        public DateTime CompletedAt { get; set; }

        /// <summary>
        /// Thời gian xử lý (milliseconds)
        /// </summary>
        public long ProcessingTimeMs => (long)(CompletedAt - StartedAt).TotalMilliseconds;

        /// <summary>
        /// ID của admin thực hiện bulk operation
        /// </summary>
        public Guid ProcessedById { get; set; }

        /// <summary>
        /// Tên admin thực hiện
        /// </summary>
        public string ProcessedByName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Kết quả xử lý từng item trong bulk operation
    /// </summary>
    public class BulkOperationItemResult
    {
        /// <summary>
        /// ID của withdrawal request
        /// </summary>
        public Guid WithdrawalRequestId { get; set; }

        /// <summary>
        /// Số tiền của yêu cầu (VNĐ)
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Tên shop
        /// </summary>
        public string ShopName { get; set; } = string.Empty;

        /// <summary>
        /// Thành công hay không
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Thông báo kết quả
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Mã lỗi (nếu có)
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// Thời gian xử lý item này
        /// </summary>
        public DateTime ProcessedAt { get; set; }
    }
}
