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

        Task<int> SaveChangesAsync();
        IDbContextTransaction BeginTransaction();
    }
}
