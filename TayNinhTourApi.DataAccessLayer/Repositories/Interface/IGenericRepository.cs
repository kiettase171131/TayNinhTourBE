using System.Linq.Expressions;
using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.DataAccessLayer.Repositories.Interface
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        Task Update(T entity);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        Task<T?> GetAsync(Expression<Func<T, bool>> predicate, string[]? include = null);
        Task<T?> GetByIdAsync(Guid id, string[]? include = null);
        Task<List<T>> GenericGetPaginationAsync(int pageIndex, int pageSize, Expression<Func<T, bool>>? predicate = null, string[]? include = null);
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? predicate = null, string[]? include = null);
        Task AddAsync(T entity);
        Task DeleteAsync(Guid id);
        Task<bool> UpdateAsync(T entity);
        Task SaveChangesAsync();
        Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate, string[]? includes = null);
        Task<List<T>> ListAsync(Expression<Func<T, bool>> predicate, string[]? includes = null);

        void DeleteRange(IEnumerable<T> entities);

        /// <summary>
        /// Get IQueryable for complex queries
        /// </summary>
        /// <returns>IQueryable of T</returns>
        IQueryable<T> GetQueryable();


    }
}
