using Microsoft.EntityFrameworkCore;
using TayNinhTourApi.DataAccessLayer.Contexts;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.DataAccessLayer.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(TayNinhTouApiDbContext context) : base(context)
        {
        }


        public async Task<bool> CheckEmailExistAsync(string email)
        {
            return await _context.Users.AnyAsync(x => x.Email == email);
        }

        public async Task<User?> FindUserByRefreshToken(Guid userId, string refreshToken)
        {
            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => (u.Id == userId) && (u.RefreshToken == refreshToken));

            return user;

        }

        public async Task<User?> GetUserByEmailAsync(string email, string[]? includes = null)
        {
            var query = _context.Users.AsQueryable();

            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            return await query.FirstOrDefaultAsync(x => x.Email == email);
        }
        public async Task<IEnumerable<User>> ListAdminsAsync()
        {
            return await _context.Users
                             .Include(u => u.Role)       // include Role để có Role.Name
                             .Where(u => u.Role!.Name == "Admin")
                             .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName)
        {
            return await _context.Users
                             .Include(u => u.Role)       // include Role để có Role.Name
                             .Where(u => u.Role!.Name == roleName)
                             .ToListAsync();
        }
        public async Task<User?> GetUserWithAllNavigationsAsync(Guid id)
        {
            return await _context.Users
                .Include(u => u.ToursCreated)
                .Include(u => u.ToursUpdated)
                .Include(u => u.TourSlotsCreated)
                .Include(u => u.TourSlotsUpdated)
                .Include(u => u.Blogs)
                .Include(u => u.BlogReactions)
                .Include(u => u.BlogComments)
                .Include(u => u.TicketsCreated)
                .Include(u => u.TicketsAssigned)
                .Include(u => u.TicketComments)
                .Include(u => u.TourOperationsAsGuide)
                .Include(u => u.TourOperationsCreated)
                .Include(u => u.TourOperationsUpdated)
                .Include(u => u.SpecialtyShop)
                .Include(u => u.TourGuide)
                .Include(u => u.TourCompany)
                .Include(u => u.ApprovedTourGuides)
                .Include(u => u.TourTemplatesCreated)
                .Include(u => u.TourTemplatesUpdated)
                .Include(u => u.TourDetailsCreated)
                .Include(u => u.TourDetailsUpdated)
                .Include(u => u.BankAccounts)
                .Include(u => u.WithdrawalRequests)
                .Include(u => u.TourBookingRefunds)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

    }
}
