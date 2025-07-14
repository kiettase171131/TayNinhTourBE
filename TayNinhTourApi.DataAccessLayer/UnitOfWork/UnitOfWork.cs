using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MySqlConnector;
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
        private ISpecialtyShopApplicationRepository _specialtyShopApplicationRepository = null!;
        private ITourGuideApplicationRepository _tourGuideApplicationRepository = null!;
        private ITourGuideRepository _tourGuideRepository = null!;
        private ITourSlotRepository _tourSlotRepository = null!;

        private ITourDetailsRepository _tourDetailsRepository = null!;
        private ITourDetailsSpecialtyShopRepository _tourDetailsSpecialtyShopRepository = null!;
        private ITourOperationRepository _tourOperationRepository = null!;
        private ITourBookingRepository _tourBookingRepository = null!;
        private ITourCompanyRepository _tourCompanyRepository = null!;
        private ITourGuideInvitationRepository _tourGuideInvitationRepository = null!;
        private ITimelineItemRepository _timelineItemRepository = null!;
        private IBlogRepository _blogRepository = null!;

        // AI Chat repositories
        private IAIChatSessionRepository _aiChatSessionRepository = null!;
        private IAIChatMessageRepository _aiChatMessageRepository = null!;

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

        public ISpecialtyShopApplicationRepository SpecialtyShopApplicationRepository
        {
            get
            {
                return _specialtyShopApplicationRepository ??= new SpecialtyShopApplicationRepository(_context);
            }
        }

        public ITourGuideApplicationRepository TourGuideApplicationRepository
        {
            get
            {
                return _tourGuideApplicationRepository ??= new TourGuideApplicationRepository(_context);
            }
        }

        public ITourGuideRepository TourGuideRepository
        {
            get
            {
                return _tourGuideRepository ??= new TourGuideRepository(_context);
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

        public ITourDetailsSpecialtyShopRepository TourDetailsSpecialtyShopRepository
        {
            get
            {
                return _tourDetailsSpecialtyShopRepository ??= new TourDetailsSpecialtyShopRepository(_context);
            }
        }

        public ITourOperationRepository TourOperationRepository
        {
            get
            {
                return _tourOperationRepository ??= new TourOperationRepository(_context);
            }
        }

        public ITourBookingRepository TourBookingRepository
        {
            get
            {
                return _tourBookingRepository ??= new TourBookingRepository(_context);
            }
        }

        public ITourCompanyRepository TourCompanyRepository
        {
            get
            {
                return _tourCompanyRepository ??= new TourCompanyRepository(_context);
            }
        }

        public ITourGuideInvitationRepository TourGuideInvitationRepository
        {
            get
            {
                return _tourGuideInvitationRepository ??= new TourGuideInvitationRepository(_context);
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

        // AI Chat repositories implementation
        public IAIChatSessionRepository AIChatSessionRepository
        {
            get
            {
                return _aiChatSessionRepository ??= new AIChatSessionRepository(_context);
            }
        }

        public IAIChatMessageRepository AIChatMessageRepository
        {
            get
            {
                return _aiChatMessageRepository ??= new AIChatMessageRepository(_context);
            }
        }

        public IDbContextTransaction BeginTransaction()
        {
            return _context.Database.BeginTransaction();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<int> ExecuteSqlRawAsync(string sql, params MySqlParameter[] parameters)
        {
            return await _context.Database.ExecuteSqlRawAsync(sql, parameters);
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
