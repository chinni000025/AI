using System.Linq.Expressions;

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
        Task<List<TEntity>?> GetByIdsAsync<Tkey>(IEnumerable<Tkey> keys, string propertyName,
            CancellationToken cancellationToken);
        Task<int> UpdatePropertyByIdsAsync<TKey, TProperty>(IEnumerable<TKey> keys,
            string idPropertyName, Expression<Func<TEntity, TProperty>> propertyExpression,
            TProperty newValue,
            CancellationToken cancellation);
        void update(TEntity entity);
        void delete(TEntity entity);
    }
}
