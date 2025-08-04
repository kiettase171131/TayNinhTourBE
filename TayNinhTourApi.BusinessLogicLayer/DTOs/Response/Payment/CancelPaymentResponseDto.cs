namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Payment
{
    /// <summary>
    /// DTO response cho cancel payment
    /// Tương tự như CancelPaymentResponseDTO trong code mẫu Java
    /// </summary>
    public class CancelPaymentResponseDto
    {
        /// <summary>
        /// Số lượng giao dịch đã hủy
        /// </summary>
        public int CancelledCount { get; set; }

        /// <summary>
        /// Thông báo kết quả
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Danh sách ID các giao dịch đã hủy
        /// </summary>
        public List<Guid> CancelledTransactionIds { get; set; } = new List<Guid>();
    }
}
