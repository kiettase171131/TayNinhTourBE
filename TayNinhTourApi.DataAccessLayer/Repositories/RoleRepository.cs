using Microsoft.EntityFrameworkCore;
using TayNinhTourApi.DataAccessLayer.Contexts;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.DataAccessLayer.Repositories
{
    public class RoleRepository : GenericRepository<Role>, IRoleRepository
    {
        public RoleRepository(TayNinhTouApiDbContext context) : base(context)
        {
        }

        public async Task<Role?> GetRoleByNameAsync(string roleUserName)
        {
            return await _context.Roles.FirstOrDefaultAsync(x => x.Name == roleUserName);
        }
    }
}
