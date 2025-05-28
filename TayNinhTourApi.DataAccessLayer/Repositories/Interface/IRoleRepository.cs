using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.DataAccessLayer.Repositories.Interface
{
    public interface IRoleRepository : IGenericRepository<Role>
    {
        Task<Role?> GetRoleByNameAsync(string roleUserName);
    }
}
