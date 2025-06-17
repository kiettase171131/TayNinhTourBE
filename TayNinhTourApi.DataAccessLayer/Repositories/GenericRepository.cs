using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TayNinhTourApi.DataAccessLayer.Contexts;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.DataAccessLayer.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        protected readonly TayNinhTouApiDbContext _context;

        public GenericRepository(TayNinhTouApiDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
        }

        public Task Update(T entity)
        {
            _context.Set<T>().Update(entity);
            return Task.CompletedTask;
        }

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().AnyAsync(predicate);
        }

        public async Task<T?> GetAsync(Expression<Func<T, bool>> predicate, string[]? include = null)
        {
            var query = _context.Set<T>().AsQueryable();
            if (include != null)
            {
                foreach (var inc in include)
                {
                    query = query.Include(inc);
                }
            }
            return await query.FirstOrDefaultAsync(predicate);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _context.Set<T>().Where(e => e.Id == id).ExecuteDeleteAsync();
        }

        public async Task<List<T>> GenericGetPaginationAsync(int pageIndex, int pageSize, Expression<Func<T, bool>>? predicate = null, string[]? include = null)
        {
            var query = _context.Set<T>().AsQueryable();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (include != null)
            {
                foreach (var inc in include)
                {
                    query = query.Include(inc);
                }
            }

            query = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? predicate = null, string[]? include = null)
        {
            var query = _context.Set<T>().AsQueryable();
            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (include != null)
            {
                foreach (var inc in include)
                {
                    query = query.Include(inc);
                }
            }

            return await query.ToListAsync();
        }

        public async Task<T?> GetByIdAsync(Guid id, string[]? include)
        {
            var query = _context.Set<T>().AsQueryable();
            if (include != null)
            {
                foreach (var inc in include)
                {
                    query = query.Include(inc);
                }
            }

            return await query.FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        public async Task<bool> UpdateAsync(T entity)
        {
            _context.Set<T>().Update(entity);
            return true;
            //await _context.SaveChangesAsync();
        }
        public async Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate, string[]? includes = null)
        {
            IQueryable<T> query = _context.Set<T>().AsQueryable();

            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            return await query.FirstOrDefaultAsync(predicate);
        }

    }
}
