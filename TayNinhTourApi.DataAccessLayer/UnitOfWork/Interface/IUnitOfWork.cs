using Microsoft.EntityFrameworkCore.Storage;
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
        ITourGuideInvitationRepository TourGuideInvitationRepository { get; }
        ITimelineItemRepository TimelineItemRepository { get; }
        IBlogRepository BlogRepository { get; }

        Task<int> SaveChangesAsync();
        IDbContextTransaction BeginTransaction();
    }
}
