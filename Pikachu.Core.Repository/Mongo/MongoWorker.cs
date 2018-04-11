using System;
using System.Collections.Generic;
using MongoDB.Driver;
using Pikachu.Core.Repository.Mongo.Interfaces;

namespace Pikachu.Core.Repository.Mongo
{
    public class MongoWorker : IMongoWorker
    {
        private Dictionary<string, dynamic> _repositories;
        private readonly IMongoDbContext _mongoDbContext;
        private readonly object _syncLock = new object();
        public IMongoDatabase Database { get; }
        public Guid InstanceId { get; }

        public MongoWorker(IMongoDbContext context)
        {
            _mongoDbContext = context;
            Database = context.Database;
            InstanceId = Guid.NewGuid();
        }

        public IMongoDbRepository<TEntity> Repository<TEntity>() where TEntity : IMongoEntity
        {
            if (_repositories == null)
            {
                _repositories = new Dictionary<string, dynamic>();
            }

            var type = typeof(TEntity).Name;

            if (_repositories.ContainsKey(type))
            {
                return (IMongoDbRepository<TEntity>)_repositories[type];
            }

            var repositoryType = typeof(MongoRepository<>);
            _repositories.Add(type,
                Activator.CreateInstance(repositoryType.MakeGenericType(typeof(TEntity)), _mongoDbContext));

            return (IMongoDbRepository<TEntity>)_repositories[type];
        }

        public IMongoDbRepository<TEntity> LockedRepository<TEntity>() where TEntity : IMongoEntity
        {
            lock (_syncLock)
            {
                if (_repositories == null)
                {
                    _repositories = new Dictionary<string, dynamic>();
                }

                var type = typeof(TEntity).Name;

                if (_repositories.ContainsKey(type))
                {
                    return (IMongoDbRepository<TEntity>) _repositories[type];
                }

                var repositoryType = typeof(MongoRepository<>);
                _repositories.Add(type,
                    Activator.CreateInstance(repositoryType.MakeGenericType(typeof(TEntity)), _mongoDbContext));

                return (IMongoDbRepository<TEntity>) _repositories[type];
            }
        }
    }
}
