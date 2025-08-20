using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MySqlConnector;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface
{
    public interface IUnitOfWork
    {
        IUserRepository UserRepository { get; }
        IRoleRepository RoleRepository { get; }
        IImageRepository ImageRepository { get; }
        ITourRepository TourRepository { get; }
        ITourTemplateRepository TourTemplateRepository { get; }
        ITourFeedbackRepository TourFeedbackRepository { get; }
        // TODO: Remove after Shop merge complete
        // IShopRepository ShopRepository { get; }
        ISpecialtyShopRepository SpecialtyShopRepository { get; }
        ISpecialtyShopApplicationRepository SpecialtyShopApplicationRepository { get; }
        ITourGuideApplicationRepository TourGuideApplicationRepository { get; }
        ITourGuideRepository TourGuideRepository { get; }
        ITourSlotRepository TourSlotRepository { get; }

        ITourDetailsRepository TourDetailsRepository { get; }
        ITourDetailsSpecialtyShopRepository TourDetailsSpecialtyShopRepository { get; }
        ITourOperationRepository TourOperationRepository { get; }
        ITourBookingRepository TourBookingRepository { get; }
        ITourBookingGuestRepository TourBookingGuestRepository { get; }
        ITourCompanyRepository TourCompanyRepository { get; }
        ITourGuideInvitationRepository TourGuideInvitationRepository { get; }
        ITourIncidentRepository TourIncidentRepository { get; }
        ITimelineItemRepository TimelineItemRepository { get; }
        IBlogRepository BlogRepository { get; }

        // Product system repositories
        IProductRepository ProductRepository { get; }

        // Notification repository
        INotificationRepository NotificationRepository { get; }

        // AI Chat repositories
        IAIChatSessionRepository AIChatSessionRepository { get; }
        IAIChatMessageRepository AIChatMessageRepository { get; }

        // Withdrawal system repositories
        IBankAccountRepository BankAccountRepository { get; }
        IWithdrawalRequestRepository WithdrawalRequestRepository { get; }

        // Tour booking refund system repositories
        ITourBookingRefundRepository TourBookingRefundRepository { get; }
        IRefundPolicyRepository RefundPolicyRepository { get; }

        // Payment system repositories
        IPaymentTransactionRepository PaymentTransactionRepository { get; }

        // Order system repositories
        IOrderRepository OrderRepository { get; }

        /// <summary>
        /// Exposes the DbContext for advanced operations like creating execution strategies
        /// </summary>
        DbContext Context { get; }

        Task<int> SaveChangesAsync();
        Task<int> ExecuteSqlRawAsync(string sql, params MySqlParameter[] parameters);
        IDbContextTransaction BeginTransaction();
        Task<IDbContextTransaction> BeginTransactionAsync();

        /// <summary>
        /// Gets the execution strategy for handling retry logic with transactions
        /// </summary>
        IExecutionStrategy GetExecutionStrategy();
        Task<T> ExecuteInStrategyAsync<T>(Func<Task<T>> operation);
    }
}
