using RealEstate_ServicesSystem.Models;
using System.Linq.Expressions;

namespace RealEstate_ServicesSystem.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
        void Update(T entity);
        void Delete(T entity);
        Task<IEnumerable<T>> GetAllAsync(
            Expression<Func<T, bool>>? expression = null,
            Expression<Func<T, object>>[]? includes = null,
            bool tracking = true,
            CancellationToken cancellationToken = default
        );
        Task<T?> GetoneAsync(
            Expression<Func<T, bool>> expression,
            Expression<Func<T, object>>[]? includes = null,
            bool tracking = true,
            CancellationToken cancellationToken = default
        );
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}