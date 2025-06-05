
using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.DataAccessLayer.Repositories.Interface
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> FindUserByRefreshToken(Guid userId, string refreshToken);
        Task<User?> GetUserByEmailAsync(string email, string[]? includes = null);
        Task<bool> CheckEmailExistAsync(string email);
        Task<IEnumerable<User>> ListAdminsAsync();
        Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName);

    }
}
