using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pikachu.Core.Repository.Sql.Interfaces
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Guid InstanceId { get; }
        TEntity Find(params object[] keyValues);
        Task<TEntity> FindAsync(params object[] keyValues);
        Task<TEntity> FindAsync(CancellationToken cancellationToken, params object[] keyValues);
        void InsertGraph(TEntity entity);
        void Update(TEntity entity);
        void UpdateMany(IEnumerable<TEntity> entities);
        void Delete(object id);
        void Delete(TEntity entity);
        void DeleteMany(IEnumerable<TEntity> entities);
        void Insert(TEntity entity);
        void InsertMany(IEnumerable<TEntity> entities);
        Task InsertManyAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken); 
        void InsertAndDetach(TEntity entity);
        IRepositoryQuery<TEntity> Query();
        IQueryable<TEntity> SqlQuery(string query, params object[] parameters);
    }
}