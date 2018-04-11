using MongoDB.Driver;

namespace Pikachu.Core.Repository.Mongo.Interfaces
{
    public interface IMongoDbContext
    {
        IMongoCollection<TEntity> GetCollection<TEntity>();
        IMongoDatabase Database { get; }
        string ConnectionString { get;  }
        string DatabaseName { get;  }
    }
}