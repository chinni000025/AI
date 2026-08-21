using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace AIEngineConnectivity.Repositories
{
    public interface IEngineRepoBase<TEntity> where TEntity : class
    {
        Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task<TEntity?> GetByIdAsync<Tkey>(Tkey key, CancellationToken cancellationToken = default);
        Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken
                cancellationToken = default);
        IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken
                = default);

        void update(TEntity entity);
        void delete(TEntity entity);
    }
}
