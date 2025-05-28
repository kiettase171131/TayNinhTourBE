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

        Task<int> SaveChangesAsync();
        IDbContextTransaction BeginTransaction();
    }
}
