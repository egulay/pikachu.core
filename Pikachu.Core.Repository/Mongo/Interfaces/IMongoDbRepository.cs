using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Pikachu.Core.Repository.Mongo.Interfaces
{
    public interface IMongoDbRepository<TEntity> where TEntity : IMongoEntity
    {
        IMongoCollection<TEntity> GetCollection();
        Task<GetOneResult<TEntity>> GetOne(ObjectId id);
        Task<GetOneResult<TEntity>> GetOne(FilterDefinition<TEntity> filter);
        Task<GetManyResult<TEntity>> GetMany(IEnumerable<ObjectId> ids, int skip, int limit);
        Task<GetManyResult<TEntity>> GetMany(FilterDefinition<TEntity> filter, int skip, int limit);
        IFindFluent<TEntity, TEntity> FindCursor(FilterDefinition<TEntity> filter);
        Task<GetManyResult<TEntity>> GetAll();
        Task<GetManyResult<TEntity>> GetAll(int skip, int limit);
        Task<bool> Exists(ObjectId id);
        Task<long> Count(ObjectId id);
        Task<long> Count(FilterDefinition<TEntity> filter);
        Task<Result> AddOne(TEntity item);
        Task<Result> AddMany(IEnumerable<TEntity> items);
        Task<Result> DeleteOne(ObjectId id);
        Task<Result> DeleteMany(IEnumerable<ObjectId> ids);
        Task<Result> UpdateOne(ObjectId id, UpdateDefinition<TEntity> update);
        Task<Result> UpdateOne(FilterDefinition<TEntity> filter, UpdateDefinition<TEntity> update);
        Task<Result> UpdateMany(IEnumerable<ObjectId> ids, UpdateDefinition<TEntity> update);
        Task<Result> UpdateMany(FilterDefinition<TEntity> filter, UpdateDefinition<TEntity> update);
        Task<GetOneResult<TEntity>> GetAndUpdateOne(FilterDefinition<TEntity> filter,
            UpdateDefinition<TEntity> update, FindOneAndUpdateOptions<TEntity, TEntity> options);
    }
}
