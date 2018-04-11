using System;
using MongoDB.Driver;
using Pikachu.Core.Repository.Mongo.Interfaces;

namespace Pikachu.Core.Repository.Mongo
{
    public class MongoDbContext : IMongoDbContext
    {
        //private readonly IMongoDatabase _mongoDatabase;
        public string ConnectionString { get; }
        public string DatabaseName { get; }
        public IMongoDatabase Database { get; }

        public MongoDbContext(Action<MongoDbContext> settings)
        {
            settings.Invoke(this);

            IMongoClient mongoClient = new MongoClient(ConnectionString);
            Database = mongoClient.GetDatabase(DatabaseName);
        }

        public MongoDbContext(string connectionString, string databaseName)
        {
            ConnectionString = connectionString;
            DatabaseName = databaseName;
            IMongoClient mongoClient = new MongoClient(connectionString);
            Database = mongoClient.GetDatabase(databaseName);
        }

        public IMongoCollection<TEntity> GetCollection<TEntity>()
        {
            return Database.GetCollection<TEntity>(string.Concat(typeof(TEntity).Name.ToLower(), "s"));
        }
    }
}
