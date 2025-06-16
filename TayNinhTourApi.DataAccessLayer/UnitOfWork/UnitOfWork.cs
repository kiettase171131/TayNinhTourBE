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
        private ITourTemplateRepository _tourTemplateRepository = null!;
        // TODO: Remove after Shop merge complete
        // private IShopRepository _shopRepository = null!;
        private ISpecialtyShopRepository _specialtyShopRepository = null!;
        private ITourSlotRepository _tourSlotRepository = null!;

        private ITourDetailsRepository _tourDetailsRepository = null!;
        private ITourOperationRepository _tourOperationRepository = null!;
        private ITimelineItemRepository _timelineItemRepository = null!;
        private IBlogRepository _blogRepository = null!;


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

        public ITourTemplateRepository TourTemplateRepository
        {
            get
            {
                return _tourTemplateRepository ??= new TourTemplateRepository(_context);
            }
        }

        // TODO: Remove after Shop merge complete
        /*
        public IShopRepository ShopRepository
        {
            get
            {
                return _shopRepository ??= new ShopRepository(_context);
            }
        }
        */

        public ISpecialtyShopRepository SpecialtyShopRepository
        {
            get
            {
                return _specialtyShopRepository ??= new SpecialtyShopRepository(_context);
            }
        }

        public ITourSlotRepository TourSlotRepository
        {
            get
            {
                return _tourSlotRepository ??= new TourSlotRepository(_context);
            }
        }



        public ITourDetailsRepository TourDetailsRepository
        {
            get
            {
                return _tourDetailsRepository ??= new TourDetailsRepository(_context);
            }
        }

        public ITourOperationRepository TourOperationRepository
        {
            get
            {
                return _tourOperationRepository ??= new TourOperationRepository(_context);
            }
        }

        public ITimelineItemRepository TimelineItemRepository
        {
            get
            {
                return _timelineItemRepository ??= new TimelineItemRepository(_context);
            }
        }

        public IBlogRepository BlogRepository
        {
            get
            {
                return _blogRepository ??= new BlogRepository(_context);
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
