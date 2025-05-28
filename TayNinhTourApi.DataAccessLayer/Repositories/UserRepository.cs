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
    }
}
