using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pikachu.Core.Repository.Helpers;
using Pikachu.Core.Repository.Sql.Interfaces;

namespace Pikachu.Core.Repository.Sql
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly DbSet<TEntity> _dbSet;
        private readonly IDbContext _context;

        public Repository(IDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<TEntity>();
            InstanceId = Guid.NewGuid();
        }

        public Guid InstanceId { get; }

        public virtual TEntity Find(params object[] keyValues)
        {
            return _dbSet.Find(keyValues);
        }

        public virtual async Task<TEntity> FindAsync(params object[] keyValues)
        {
            return await _dbSet.FindAsync(keyValues);
        }

        public virtual async Task<TEntity> FindAsync(CancellationToken cancellationToken, params object[] keyValues)
        {
            return await _dbSet.FindAsync(cancellationToken, keyValues);
        }

        public virtual void InsertGraph(TEntity entity)
        {
            _dbSet.Add(entity);
        }

        public virtual void Update(TEntity entity)
        {
            _dbSet.Attach(entity);
            ((IObjectState)entity).State = ObjectState.Modified;
        }

        public virtual void UpdateMany(IEnumerable<TEntity> entities)
        {
            var enumerable = entities as TEntity[] ?? entities.ToArray();
            foreach (var entity in enumerable)
            {
                ((IObjectState)entity).State = ObjectState.Modified;
            }
            _dbSet.UpdateRange(enumerable);
        }

        public virtual void Delete(object id)
        {
            var entity = _dbSet.Find(id);
            Delete(entity);
        }

        public virtual void Delete(TEntity entity)
        {
            _dbSet.Attach(entity);
            ((IObjectState)entity).State = ObjectState.Deleted;
            _dbSet.Remove(entity);
        }

        public void DeleteMany(IEnumerable<TEntity> entities)
        {
            var enumerable = entities as TEntity[] ?? entities.ToArray();
            foreach (var entity in enumerable)
            {
                ((IObjectState)entity).State = ObjectState.Deleted;
            }
            _dbSet.RemoveRange(enumerable);
        }

        public virtual void Insert(TEntity entity)
        {
            _dbSet.Attach(entity);
            ((IObjectState)entity).State = ObjectState.Added;
        }


        public virtual void InsertMany(IEnumerable<TEntity> entities)
        {
            var enumerable = entities as TEntity[] ?? entities.ToArray();
            foreach (var entity in enumerable)
            {
                ((IObjectState)entity).State = ObjectState.Added;
            }
            _dbSet.AddRange(enumerable);
        }

        public virtual Task InsertManyAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
        {
            var enumerable = entities as TEntity[] ?? entities.ToArray();
            foreach (var entity in enumerable)
            {
                ((IObjectState)entity).State = ObjectState.Added;
            }
            return _dbSet.AddRangeAsync(enumerable, cancellationToken);
        }

        public virtual void InsertAndDetach(TEntity entity)
        {
            _dbSet.Attach(entity);
            ((IObjectState)entity).State = ObjectState.Added;

            _context.SaveChanges();
            ((IObjectState) entity).State = ObjectState.Detached;
        }

        public virtual IRepositoryQuery<TEntity> Query()
        {
            var repositoryGetFluentHelper = new RepositoryQuery<TEntity>(this);
            return repositoryGetFluentHelper;
        }

        public virtual IQueryable<TEntity> SqlQuery(string query, params object[] parameters)
        {
            return _dbSet.FromSql(query, parameters).AsQueryable();
        }

        internal IQueryable<TEntity> Get(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            List<Expression<Func<TEntity, object>>> includeProperties = null,
            int? page = null,
            int? pageSize = null)
        {
            IQueryable<TEntity> query = _dbSet;

            includeProperties?.ForEach(i => query = query.Include(i));

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            if (page != null && pageSize != null)
            {
                query = query.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value);
            }
            return query;
        }

        internal async Task<IEnumerable<TEntity>> GetAsync(
                    Expression<Func<TEntity, bool>> filter = null,
                    Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
                    List<Expression<Func<TEntity, object>>> includeProperties = null,
                    int? page = null,
                    int? pageSize = null)
        {
            return Get(filter, orderBy, includeProperties, page, pageSize).AsEnumerable();
        }
    }
}