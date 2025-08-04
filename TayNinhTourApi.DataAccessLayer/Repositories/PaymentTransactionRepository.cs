using Microsoft.EntityFrameworkCore;
using TayNinhTourApi.DataAccessLayer.Contexts;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.DataAccessLayer.Repositories
{
    /// <summary>
    /// Repository implementation cho PaymentTransaction
    /// </summary>
    public class PaymentTransactionRepository : GenericRepository<PaymentTransaction>, IPaymentTransactionRepository
    {
        public PaymentTransactionRepository(TayNinhTouApiDbContext context) : base(context)
        {
        }

        public async Task<PaymentTransaction?> FindRootTransactionByOrderIdAsync(Guid orderId)
        {
            return await _context.PaymentTransactions
                .Where(t => t.OrderId == orderId && t.ParentTransactionId == null)
                .FirstOrDefaultAsync();
        }

        public async Task<PaymentTransaction?> FindRootTransactionByTourBookingIdAsync(Guid tourBookingId)
        {
            return await _context.PaymentTransactions
                .Where(t => t.TourBookingId == tourBookingId && t.ParentTransactionId == null)
                .FirstOrDefaultAsync();
        }

        public async Task<PaymentTransaction?> FindByPayOsTransactionIdAsync(string payOsTransactionId)
        {
            return await _context.PaymentTransactions
                .Where(t => t.PayOsTransactionId == payOsTransactionId)
                .FirstOrDefaultAsync();
        }

        public async Task<PaymentTransaction?> FindByParentTransactionIdAsync(Guid parentTransactionId)
        {
            return await _context.PaymentTransactions
                .Where(t => t.ParentTransactionId == parentTransactionId)
                .FirstOrDefaultAsync();
        }

        public async Task<List<PaymentTransaction>> FindAllByOrderIdAsync(Guid orderId)
        {
            return await _context.PaymentTransactions
                .Where(t => t.OrderId == orderId)
                .OrderBy(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<PaymentTransaction>> FindAllByTourBookingIdAsync(Guid tourBookingId)
        {
            return await _context.PaymentTransactions
                .Where(t => t.TourBookingId == tourBookingId)
                .OrderBy(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> ExistsByOrderAndGatewayAndStatusAsync(Guid orderId, PaymentGateway gateway, PaymentStatus status)
        {
            return await _context.PaymentTransactions
                .AnyAsync(t => t.OrderId == orderId && t.Gateway == gateway && t.Status == status);
        }

        public async Task<bool> ExistsByTourBookingAndGatewayAndStatusAsync(Guid tourBookingId, PaymentGateway gateway, PaymentStatus status)
        {
            return await _context.PaymentTransactions
                .AnyAsync(t => t.TourBookingId == tourBookingId && t.Gateway == gateway && t.Status == status);
        }

        public async Task<List<PaymentTransaction>> FindByPayOsOrderCodeAsync(string payOsOrderCode)
        {
            if (string.IsNullOrWhiteSpace(payOsOrderCode))
                return new List<PaymentTransaction>();

            // Try direct string comparison first
            var transactions = await _context.PaymentTransactions
                .Where(t => t.PayOsOrderCode == payOsOrderCode)
                .OrderBy(t => t.CreatedAt)
                .ToListAsync();

            if (transactions.Any())
                return transactions;

            // Fallback: try numeric comparison for legacy data
            if (long.TryParse(payOsOrderCode, out long orderCodeLong))
            {
                return await _context.PaymentTransactions
                    .Where(t => t.PayOsOrderCode == orderCodeLong.ToString())
                    .OrderBy(t => t.CreatedAt)
                    .ToListAsync();
            }

            return new List<PaymentTransaction>();
        }

        public async Task<string> GetPaymentTypeAsync(Guid transactionId)
        {
            var transaction = await _context.PaymentTransactions
                .Where(t => t.Id == transactionId)
                .Select(t => new { t.OrderId, t.TourBookingId })
                .FirstOrDefaultAsync();

            if (transaction == null)
                return "Unknown";

            if (transaction.OrderId.HasValue)
                return "ProductPayment";

            if (transaction.TourBookingId.HasValue)
                return "TourBookingPayment";

            return "Unknown";
        }

        public async Task<List<PaymentTransaction>> GetProductPaymentTransactionsAsync(int pageIndex = 0, int pageSize = 10)
        {
            return await _context.PaymentTransactions
                .Where(t => t.OrderId != null && t.TourBookingId == null)
                .OrderByDescending(t => t.CreatedAt)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .Include(t => t.Order)
                .ToListAsync();
        }

        public async Task<List<PaymentTransaction>> GetTourBookingPaymentTransactionsAsync(int pageIndex = 0, int pageSize = 10)
        {
            return await _context.PaymentTransactions
                .Where(t => t.TourBookingId != null && t.OrderId == null)
                .OrderByDescending(t => t.CreatedAt)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .Include(t => t.TourBooking)
                .ToListAsync();
        }

        public async Task<PaymentTransaction?> GetByPayOsOrderCodeAsync(string payOsOrderCode)
        {
            if (string.IsNullOrWhiteSpace(payOsOrderCode))
                return null;

            // First try direct string comparison (for TNDT format)
            var transaction = await _context.PaymentTransactions
                .Where(t => t.PayOsOrderCode == payOsOrderCode)
                .Include(t => t.Order)
                .Include(t => t.TourBooking)
                .FirstOrDefaultAsync();

            if (transaction != null)
                return transaction;

            // Fallback: try numeric comparison for legacy data
            if (long.TryParse(payOsOrderCode, out long orderCodeLong))
            {
                return await _context.PaymentTransactions
                    .Where(t => t.PayOsOrderCode == orderCodeLong.ToString())
                    .Include(t => t.Order)
                    .Include(t => t.TourBooking)
                    .FirstOrDefaultAsync();
            }

            return null;
        }

        public async Task<PaymentTransaction?> GetByOrderIdAsync(Guid orderId)
        {
            return await _context.PaymentTransactions
                .Where(t => t.OrderId == orderId)
                .Include(t => t.Order)
                .FirstOrDefaultAsync();
        }

        public async Task<PaymentTransaction?> GetByTourBookingIdAsync(Guid tourBookingId)
        {
            return await _context.PaymentTransactions
                .Where(t => t.TourBookingId == tourBookingId)
                .Include(t => t.TourBooking)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<PaymentTransaction>> GetByOrderIdAllAsync(Guid orderId)
        {
            return await _context.PaymentTransactions
                .Where(t => t.OrderId == orderId)
                .Include(t => t.Order)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentTransaction>> GetByTourBookingIdAllAsync(Guid tourBookingId)
        {
            return await _context.PaymentTransactions
                .Where(t => t.TourBookingId == tourBookingId)
                .Include(t => t.TourBooking)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }
    }
}
