using Microsoft.EntityFrameworkCore.Storage;
using TayNinhTourApi.DataAccessLayer.Contexts;
using TayNinhTourApi.DataAccessLayer.Repositories;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;

namespace TayNinhTourApi.DataAccessLayer.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private bool _disposed = false;

        private readonly TayNinhTouApiDbContext _context;

        private IUserRepository _userRepository = null!;
        private IRoleRepository _roleRepository = null!;
        private IImageRepository _imageRepository = null!;
        private ITourRepository _tourRepository = null!;

        public UnitOfWork(TayNinhTouApiDbContext context)
        {
            _context = context;
        }

        IUserRepository IUnitOfWork.UserRepository
        {
            get
            {
                return _userRepository ??= new UserRepository(_context);
            }
        }

        IRoleRepository IUnitOfWork.RoleRepository
        {
            get
            {
                return _roleRepository ??= new RoleRepository(_context);
            }
        }

        public IImageRepository ImageRepository
        {
            get
            {
                return _imageRepository ??= new ImageRepository(_context);
            }
        }

        public ITourRepository TourRepository
        {
            get
            {
                return _tourRepository ??= new TourRepository(_context);
            }
        }

        public IDbContextTransaction BeginTransaction()
        {
            return _context.Database.BeginTransaction();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
