using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.DataAccessLayer.Repositories.Interface
{
    /// <summary>
    /// Repository interface cho TourBookingRefund entity
    /// Cung cấp các methods để truy cập và thao tác với dữ liệu TourBookingRefund
    /// </summary>
    public interface ITourBookingRefundRepository : IGenericRepository<TourBookingRefund>
    {
        /// <summary>
        /// Lấy danh sách yêu cầu hoàn tiền của một customer (user phổ thông)
        /// </summary>
        /// <param name="userId">ID của customer</param>
        /// <param name="status">Lọc theo trạng thái (null = tất cả)</param>
        /// <param name="refundType">Lọc theo loại hoàn tiền (null = tất cả)</param>
        /// <param name="pageNumber">Số trang</param>
        /// <param name="pageSize">Kích thước trang</param>
        /// <returns>Danh sách yêu cầu hoàn tiền với thông tin tour booking</returns>
        Task<(IEnumerable<TourBookingRefund> Items, int TotalCount)> GetByCustomerIdAsync(
            Guid userId,
            TourRefundStatus? status = null,
            TourRefundType? refundType = null,
            int pageNumber = 1,
            int pageSize = 10);

        /// <summary>
        /// Lấy danh sách yêu cầu hoàn tiền cho admin
        /// </summary>
        /// <param name="status">Lọc theo trạng thái (null = tất cả)</param>
        /// <param name="refundType">Lọc theo loại hoàn tiền (null = tất cả)</param>
        /// <param name="pageNumber">Số trang</param>
        /// <param name="pageSize">Kích thước trang</param>
        /// <param name="searchTerm">Từ khóa tìm kiếm (tên customer, booking code, etc.)</param>
        /// <param name="fromDate">Lọc từ ngày</param>
        /// <param name="toDate">Lọc đến ngày</param>
        /// <returns>Danh sách yêu cầu hoàn tiền với đầy đủ thông tin</returns>
        Task<(IEnumerable<TourBookingRefund> Items, int TotalCount)> GetForAdminAsync(
            TourRefundStatus? status = null,
            TourRefundType? refundType = null,
            int pageNumber = 1,
            int pageSize = 10,
            string? searchTerm = null,
            DateTime? fromDate = null,
            DateTime? toDate = null);

        /// <summary>
        /// Lấy yêu cầu hoàn tiền theo ID với đầy đủ thông tin
        /// </summary>
        /// <param name="refundId">ID của yêu cầu hoàn tiền</param>
        /// <returns>Yêu cầu hoàn tiền với navigation properties (TourBooking, User, ProcessedBy)</returns>
        Task<TourBookingRefund?> GetWithDetailsAsync(Guid refundId);

        /// <summary>
        /// Lấy yêu cầu hoàn tiền theo ID với kiểm tra ownership
        /// </summary>
        /// <param name="refundId">ID của yêu cầu hoàn tiền</param>
        /// <param name="customerId">ID của customer (để kiểm tra ownership)</param>
        /// <returns>Yêu cầu hoàn tiền nếu customer là owner, null nếu không</returns>
        Task<TourBookingRefund?> GetByIdAndCustomerIdAsync(Guid refundId, Guid customerId);

        /// <summary>
        /// Lấy yêu cầu hoàn tiền theo TourBooking ID
        /// </summary>
        /// <param name="tourBookingId">ID của tour booking</param>
        /// <returns>Yêu cầu hoàn tiền nếu có, null nếu không</returns>
        Task<TourBookingRefund?> GetByTourBookingIdAsync(Guid tourBookingId);

        /// <summary>
        /// Kiểm tra tour booking đã có yêu cầu hoàn tiền chưa
        /// </summary>
        /// <param name="tourBookingId">ID của tour booking</param>
        /// <returns>True nếu đã có yêu cầu hoàn tiền</returns>
        Task<bool> HasRefundRequestAsync(Guid tourBookingId);

        /// <summary>
        /// Kiểm tra customer có yêu cầu hoàn tiền pending nào không
        /// </summary>
        /// <param name="customerId">ID của customer</param>
        /// <returns>True nếu có yêu cầu pending</returns>
        Task<bool> HasPendingRefundAsync(Guid customerId);

        /// <summary>
        /// Đếm số lượng yêu cầu hoàn tiền theo trạng thái
        /// </summary>
        /// <param name="status">Trạng thái cần đếm</param>
        /// <param name="refundType">Loại hoàn tiền (null = tất cả)</param>
        /// <returns>Số lượng yêu cầu</returns>
        Task<int> CountByStatusAsync(TourRefundStatus status, TourRefundType? refundType = null);

        /// <summary>
        /// Lấy tổng số tiền đang chờ hoàn (status = Pending hoặc Approved)
        /// </summary>
        /// <param name="refundType">Loại hoàn tiền (null = tất cả)</param>
        /// <returns>Tổng số tiền pending</returns>
        Task<decimal> GetTotalPendingRefundAmountAsync(TourRefundType? refundType = null);

        /// <summary>
        /// Lấy tổng số tiền đã hoàn trong khoảng thời gian
        /// </summary>
        /// <param name="fromDate">Từ ngày</param>
        /// <param name="toDate">Đến ngày</param>
        /// <param name="refundType">Loại hoàn tiền (null = tất cả)</param>
        /// <returns>Tổng số tiền đã hoàn</returns>
        Task<decimal> GetTotalRefundedAmountAsync(DateTime fromDate, DateTime toDate, TourRefundType? refundType = null);

        /// <summary>
        /// Lấy yêu cầu hoàn tiền gần nhất của customer
        /// </summary>
        /// <param name="customerId">ID của customer</param>
        /// <returns>Yêu cầu hoàn tiền gần nhất</returns>
        Task<TourBookingRefund?> GetLatestByCustomerIdAsync(Guid customerId);

        /// <summary>
        /// Lấy danh sách yêu cầu hoàn tiền theo tour company
        /// </summary>
        /// <param name="tourCompanyId">ID của tour company</param>
        /// <param name="status">Lọc theo trạng thái (null = tất cả)</param>
        /// <param name="pageNumber">Số trang</param>
        /// <param name="pageSize">Kích thước trang</param>
        /// <returns>Danh sách yêu cầu hoàn tiền liên quan đến tour company</returns>
        Task<(IEnumerable<TourBookingRefund> Items, int TotalCount)> GetByTourCompanyIdAsync(
            Guid tourCompanyId,
            TourRefundStatus? status = null,
            int pageNumber = 1,
            int pageSize = 10);

        /// <summary>
        /// Cập nhật trạng thái yêu cầu hoàn tiền
        /// </summary>
        /// <param name="refundId">ID của yêu cầu</param>
        /// <param name="status">Trạng thái mới</param>
        /// <param name="processedById">ID của admin xử lý</param>
        /// <param name="approvedAmount">Số tiền được duyệt</param>
        /// <param name="adminNotes">Ghi chú từ admin</param>
        /// <param name="transactionReference">Mã tham chiếu giao dịch</param>
        /// <returns>True nếu cập nhật thành công</returns>
        Task<bool> UpdateStatusAsync(
            Guid refundId,
            TourRefundStatus status,
            Guid? processedById = null,
            decimal? approvedAmount = null,
            string? adminNotes = null,
            string? transactionReference = null);

        /// <summary>
        /// Lấy thống kê hoàn tiền theo tháng
        /// </summary>
        /// <param name="year">Năm</param>
        /// <param name="month">Tháng</param>
        /// <param name="refundType">Loại hoàn tiền (null = tất cả)</param>
        /// <returns>Thống kê hoàn tiền</returns>
        Task<(int TotalRequests, decimal TotalRequestedAmount, int CompletedRequests, decimal TotalRefundedAmount)> GetMonthlyStatsAsync(
            int year, 
            int month, 
            TourRefundType? refundType = null);

        /// <summary>
        /// Lấy danh sách yêu cầu hoàn tiền cần xử lý tự động (auto-cancellation)
        /// </summary>
        /// <param name="tourOperationIds">Danh sách ID của tour operations bị hủy</param>
        /// <returns>Danh sách tour bookings cần tạo refund request</returns>
        Task<IEnumerable<TourBooking>> GetBookingsForAutoCancellationAsync(IEnumerable<Guid> tourOperationIds);

        /// <summary>
        /// Tạo bulk refund requests cho auto-cancellation
        /// </summary>
        /// <param name="refundRequests">Danh sách refund requests cần tạo</param>
        /// <returns>True nếu tạo thành công</returns>
        Task<bool> CreateBulkRefundRequestsAsync(IEnumerable<TourBookingRefund> refundRequests);

        /// <summary>
        /// Lấy danh sách yêu cầu hoàn tiền theo admin processor
        /// </summary>
        /// <param name="adminId">ID của admin</param>
        /// <param name="fromDate">Từ ngày</param>
        /// <param name="toDate">Đến ngày</param>
        /// <returns>Danh sách yêu cầu đã xử lý bởi admin</returns>
        Task<IEnumerable<TourBookingRefund>> GetProcessedByAdminAsync(Guid adminId, DateTime? fromDate = null, DateTime? toDate = null);

        /// <summary>
        /// Kiểm tra tour booking có đủ điều kiện để hoàn tiền không
        /// </summary>
        /// <param name="tourBookingId">ID của tour booking</param>
        /// <returns>True nếu đủ điều kiện hoàn tiền</returns>
        Task<bool> IsEligibleForRefundAsync(Guid tourBookingId);

        /// <summary>
        /// Lấy average processing time cho refund requests
        /// </summary>
        /// <param name="refundType">Loại hoàn tiền (null = tất cả)</param>
        /// <param name="fromDate">Từ ngày</param>
        /// <param name="toDate">Đến ngày</param>
        /// <returns>Thời gian xử lý trung bình (giờ)</returns>
        Task<double> GetAverageProcessingTimeAsync(TourRefundType? refundType = null, DateTime? fromDate = null, DateTime? toDate = null);
    }
}
