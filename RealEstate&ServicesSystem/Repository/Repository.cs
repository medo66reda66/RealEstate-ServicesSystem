
using Microsoft.EntityFrameworkCore;
using RealEstate_ServicesSystem.DATABS;
using RealEstate_ServicesSystem.Repository.IRepository;
using System.Linq.Expressions;

namespace RealEstate_ServicesSystem.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDBcontext _context;
        private readonly DbSet<T> _dbSet;
        public Repository(ApplicationDBcontext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }
        public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            var result = await _dbSet.AddAsync(entity, cancellationToken);
            return result.Entity;
        }
        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }
        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }
        public async Task<IEnumerable<T>> GetAllAsync(
           Expression<Func<T, bool>>? expression,
           Expression<Func<T, object>>[]? includes,
           bool tracking = true,
           CancellationToken cancellationToken = default
            )
        {
            var query = _dbSet.AsQueryable();

            if (expression != null)
            {
                query = query.Where(expression);
            }

            if (includes != null)
            {
                foreach (var include in includes)
                    query = query.Include(include);
            }

            if (!tracking)
            {
                query = query.AsNoTracking();
            }

            return await query.ToListAsync(cancellationToken);
        }
        public async Task<T?> GetoneAsync(
            Expression<Func<T, bool>> expression,
            Expression<Func<T, object>>[]? includes,
            bool tracking = true,
            CancellationToken cancellationToken = default
            )
        {
            return (await GetAllAsync(expression, includes, tracking, cancellationToken)).FirstOrDefault();
        }
        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                // Handle database update exceptions
                Console.WriteLine($"Database update error: {ex.Message}");
                throw;
            }
        }
    }
}
