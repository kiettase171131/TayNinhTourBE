using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.DataAccessLayer.Repositories.Interface
{
    /// <summary>
    /// Repository interface cho PaymentTransaction
    /// </summary>
    public interface IPaymentTransactionRepository : IGenericRepository<PaymentTransaction>
    {
        /// <summary>
        /// Tìm transaction gốc theo OrderId
        /// </summary>
        Task<PaymentTransaction?> FindRootTransactionByOrderIdAsync(Guid orderId);

        /// <summary>
        /// Tìm transaction gốc theo TourBookingId
        /// </summary>
        Task<PaymentTransaction?> FindRootTransactionByTourBookingIdAsync(Guid tourBookingId);

        /// <summary>
        /// Tìm transaction theo PayOS Transaction ID
        /// </summary>
        Task<PaymentTransaction?> FindByPayOsTransactionIdAsync(string payOsTransactionId);

        /// <summary>
        /// Tìm transaction con theo Parent Transaction ID
        /// </summary>
        Task<PaymentTransaction?> FindByParentTransactionIdAsync(Guid parentTransactionId);

        /// <summary>
        /// Lấy tất cả transactions theo OrderId
        /// </summary>
        Task<List<PaymentTransaction>> FindAllByOrderIdAsync(Guid orderId);

        /// <summary>
        /// Lấy tất cả transactions theo TourBookingId
        /// </summary>
        Task<List<PaymentTransaction>> FindAllByTourBookingIdAsync(Guid tourBookingId);

        /// <summary>
        /// Kiểm tra có transaction PAID cho Order và Gateway không
        /// </summary>
        Task<bool> ExistsByOrderAndGatewayAndStatusAsync(Guid orderId, PaymentGateway gateway, PaymentStatus status);

        /// <summary>
        /// Kiểm tra có transaction PAID cho TourBooking và Gateway không
        /// </summary>
        Task<bool> ExistsByTourBookingAndGatewayAndStatusAsync(Guid tourBookingId, PaymentGateway gateway, PaymentStatus status);

        /// <summary>
        /// Lấy transactions theo PayOS Order Code
        /// </summary>
        Task<List<PaymentTransaction>> FindByPayOsOrderCodeAsync(string payOsOrderCode);

        /// <summary>
        /// Kiểm tra transaction là product payment hay tour booking payment
        /// </summary>
        Task<string> GetPaymentTypeAsync(Guid transactionId);

        /// <summary>
        /// Lấy tất cả product payment transactions
        /// </summary>
        Task<List<PaymentTransaction>> GetProductPaymentTransactionsAsync(int pageIndex = 0, int pageSize = 10);

        /// <summary>
        /// Lấy tất cả tour booking payment transactions
        /// </summary>
        Task<List<PaymentTransaction>> GetTourBookingPaymentTransactionsAsync(int pageIndex = 0, int pageSize = 10);

        /// <summary>
        /// Lấy transaction theo PayOS order code
        /// </summary>
        Task<PaymentTransaction?> GetByPayOsOrderCodeAsync(string payOsOrderCode);

        /// <summary>
        /// Lấy transaction theo Order ID
        /// </summary>
        Task<PaymentTransaction?> GetByOrderIdAsync(Guid orderId);

        /// <summary>
        /// Lấy transaction theo Tour Booking ID
        /// </summary>
        Task<PaymentTransaction?> GetByTourBookingIdAsync(Guid tourBookingId);

        /// <summary>
        /// Lấy tất cả transactions theo Order ID
        /// </summary>
        Task<IEnumerable<PaymentTransaction>> GetByOrderIdAllAsync(Guid orderId);

        /// <summary>
        /// Lấy tất cả transactions theo Tour Booking ID
        /// </summary>
        Task<IEnumerable<PaymentTransaction>> GetByTourBookingIdAllAsync(Guid tourBookingId);
    }
}
