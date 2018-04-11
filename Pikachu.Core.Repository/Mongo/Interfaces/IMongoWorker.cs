using MongoDB.Driver;

namespace Pikachu.Core.Repository.Mongo.Interfaces
{
    public interface IMongoWorker
    {
        IMongoDatabase Database { get; }
        IMongoDbRepository<TEntity> Repository<TEntity>() where TEntity : IMongoEntity;
        IMongoDbRepository<TEntity> LockedRepository<TEntity>() where TEntity : IMongoEntity;
    }
}