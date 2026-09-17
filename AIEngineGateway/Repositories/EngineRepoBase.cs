using AIEngineConnectivity.Repositories;
using AIEngineGateway.EngineInfrastructure;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AIEngineGateway.Repositories
{
    public class EngineRepoBase<TEntity> : IEngineRepoBase<TEntity> where TEntity : class
    {
        protected DbSet<TEntity> _dbSet;
        protected EngineContext _engineContext;
        public EngineRepoBase(EngineContext engineContext)
        {
            _engineContext = engineContext;
            _dbSet = _engineContext.Set<TEntity>();
        }
        public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            var entry = await _dbSet.AddAsync(entity, cancellationToken);
            return entry.Entity;
        }

        public void delete(TEntity entity)
        {
            _dbSet.Remove(entity);
        }

        public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
        }

        public Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<TEntity?> GetByIdAsync<TKey>(TKey key, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FindAsync(key, cancellationToken);
        }

        public IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<List<TEntity>?> GetByIdsAsync<Tkey>(IEnumerable<Tkey> keys, string propertyName,
            CancellationToken cancellationToken)
        {
            return await _dbSet.Where(x => keys.Contains(EF.Property<Tkey>(x, propertyName)))
            .ToListAsync(cancellationToken);
        }

        public void update(TEntity entity)
        {
            _dbSet.Update(entity);
        }

        public async Task<int> UpdatePropertyByIdsAsync<TKey, TProperty>(IEnumerable<TKey> keys,
            string idPropertyName, Expression<Func<TEntity, TProperty>> propertyExpression,
            TProperty newValue,
            CancellationToken cancellation)
        {
            return await _dbSet.Where(x => keys.Contains(EF.Property<TKey>(x, idPropertyName)))
                .ExecuteUpdateAsync(s => s.SetProperty(propertyExpression, newValue), cancellation);
        }
    }
}
