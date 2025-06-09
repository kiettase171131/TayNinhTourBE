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
        IShopRepository ShopRepository { get; }
        ITourSlotRepository TourSlotRepository { get; }
        ITourDetailsRepository TourDetailsRepository { get; }
        ITourOperationRepository TourOperationRepository { get; }
        ITimelineItemRepository TimelineItemRepository { get; }
        IBlogRepository BlogRepository { get; }

        Task<int> SaveChangesAsync();
        IDbContextTransaction BeginTransaction();
    }
}
