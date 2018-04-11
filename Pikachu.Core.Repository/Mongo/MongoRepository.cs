using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Pikachu.Core.Repository.Helpers;
using Pikachu.Core.Repository.Mongo.Interfaces;

namespace Pikachu.Core.Repository.Mongo
{
    public class MongoRepository<TEntity> : IMongoDbRepository<TEntity> where TEntity : IMongoEntity
    {
        private readonly IMongoDbContext _mongoDbContext;
        public MongoRepository(IMongoDbContext mongoDbContext)
        {
            _mongoDbContext = mongoDbContext;
        }

        public IMongoCollection<TEntity> GetCollection()
        {
            return _mongoDbContext.GetCollection<TEntity>();
        }

        #region Get

        public async Task<GetOneResult<TEntity>> GetOne(ObjectId id)
        {
            var filter = Builders<TEntity>.Filter.Eq("_id", id);
            return await GetOne(filter);
        }

        public async Task<GetOneResult<TEntity>> GetOne(FilterDefinition<TEntity> filter)
        {
            var res = new GetOneResult<TEntity>();
            try
            {
                var collection = GetCollection();
                var entity = await collection.Find(filter).SingleOrDefaultAsync();
                if (entity != null)
                    res.Entity = entity;
                
                res.Success = true;
                return res;
            }
            catch (Exception ex)
            {
                res.Message = ExceptionHelper.NotifyException("GetOne",
                    string.Concat("Exception getting one ", typeof(TEntity).Name),
                    ex);
                return res;
            }
        }

        public async Task<GetManyResult<TEntity>> GetMany(IEnumerable<ObjectId> ids, int skip, int limit)
        {
            try
            {
                //var collection = GetCollection<TEntity>();
                var filter = Builders<TEntity>.Filter.Eq("_id", ids);
                return await GetMany(filter, skip, limit);
            }
            catch (Exception ex)
            {
                var res = new GetManyResult<TEntity>
                {
                    Message = ExceptionHelper.NotifyException("GetMany",
                        string.Concat("Exception getting many ", typeof(TEntity).Name, "s"), ex)
                };
                return res;
            }
        }

        public async Task<GetManyResult<TEntity>> GetMany(FilterDefinition<TEntity> filter, int skip, int limit)
        {
            var res = new GetManyResult<TEntity>();
            try
            {
                var collection = GetCollection();
                var entities = await collection.Find(filter).Skip(skip).Limit(limit).ToListAsync();
                if (entities != null)
                {
                    res.Entities = entities;
                    res.TotalCount = entities.Count;
                }
                res.Success = true;
                return res;
            }
            catch (Exception ex)
            {
                res.Message = ExceptionHelper.NotifyException("GetMany",
                    string.Concat("Exception getting many ", typeof(TEntity).Name, "s"), ex);
                return res;
            }
        }

        public IFindFluent<TEntity, TEntity> FindCursor(FilterDefinition<TEntity> filter)
        {
            //var res = new GetManyResult<TEntity>();
            var collection = GetCollection();
            var cursor = collection.Find(filter);
            return cursor;
        }

        public async Task<GetManyResult<TEntity>> GetAll(int skip, int limit)
        {
            var res = new GetManyResult<TEntity>();
            try
            {
                var collection = GetCollection();
                var entities = await collection.Find(new BsonDocument()).Skip(skip).Limit(limit).ToListAsync();
                if (entities != null)
                {
                    res.Entities = entities;
                    res.TotalCount = entities.Count;
                }

                res.Success = true;
                return res;
            }
            catch (Exception ex)
            {
                res.Message = ExceptionHelper.NotifyException("GetAll",
                    string.Concat("Exception getting all ", typeof(TEntity).Name, "s"), ex);
                return res;
            }
        }

        public async Task<GetManyResult<TEntity>> GetAll()
        {
            var res = new GetManyResult<TEntity>();
            try
            {
                var collection = GetCollection();
                var entities = await collection.Find(new BsonDocument()).ToListAsync();
                if (entities != null)
                {
                    res.Entities = entities;
                    res.TotalCount = entities.Count;
                }

                res.Success = true;
                return res;
            }
            catch (Exception ex)
            {
                res.Message = ExceptionHelper.NotifyException("GetAll",
                    string.Concat("Exception getting all ", typeof(TEntity).Name, "s"), ex);
                return res;
            }
        }

        public async Task<bool> Exists(ObjectId id)
        {
            var collection = GetCollection();
            var query = new BsonDocument("_id", id);
            var cursor = collection.Find(query);
            var count = await cursor.CountAsync();
            return (count > 0);
        }


        public async Task<long> Count(ObjectId id)
        {
            var filter = new FilterDefinitionBuilder<TEntity>().Eq("_id", id);
            return await Count(filter);
        }

        public async Task<long> Count(FilterDefinition<TEntity> filter)
        {
            var collection = GetCollection();
            var cursor = collection.Find(filter);
            var count = await cursor.CountAsync();
            return count;
        }

        #endregion Get

        #region Create

        public async Task<Result> AddOne(TEntity item)
        {
            var res = new Result();
            try
            {
                var collection = GetCollection();
                await collection.InsertOneAsync(item);
                res.Success = true;
                res.Message = "OK";
                return res;
            }
            catch (Exception ex)
            {
                res.Message = ExceptionHelper.NotifyException("AddOne",
                    string.Concat("Exception adding one ", typeof(TEntity).Name),
                    ex);
                return res;
            }
        }

        public async Task<Result> AddMany(IEnumerable<TEntity> items)
        {
            var res = new Result();
            try
            {
                var collection = GetCollection();
                await collection.InsertManyAsync(items);
                res.Success = true;
                res.Message = "OK";
                return res;
            }
            catch (Exception ex)
            {
                res.Message = ExceptionHelper.NotifyException("AddMany",
                    string.Concat("Exception adding many ", typeof(TEntity).Name),
                    ex);
                return res;
            }
        }

        #endregion Create

        #region Delete
        public async Task<Result> DeleteOne(ObjectId id)
        {
            var filter = new FilterDefinitionBuilder<TEntity>().Eq("_id", id);
            return await DeleteOne(filter);
        }

        public async Task<Result> DeleteOne(FilterDefinition<TEntity> filter)
        {
            var result = new Result();
            try
            {
                var collection = GetCollection();
                await collection.DeleteOneAsync(filter);
                result.Success = true;
                result.Message = "OK";
                return result;
            }
            catch (Exception ex)
            {
                result.Message = ExceptionHelper.NotifyException("DeleteOne",
                    string.Concat("Exception deleting one ", typeof(TEntity).Name), ex);
                return result;
            }
        }

        public async Task<Result> DeleteMany(IEnumerable<ObjectId> ids)
        {
            var filter = new FilterDefinitionBuilder<TEntity>().In("_id", ids);
            return await DeleteMany(filter);
        }

        public async Task<Result> DeleteMany(FilterDefinition<TEntity> filter)
        {
            var result = new Result();
            try
            {
                var collection = GetCollection();
                var deleteRes = await collection.DeleteManyAsync(filter);
                if (deleteRes.DeletedCount < 1)
                {
                    var ex = new Exception();
                    result.Message = ExceptionHelper.NotifyException("DeleteMany",
                        string.Concat("Some ", typeof(TEntity).Name, "s could not be deleted."), ex);
                    return result;
                }
                result.Success = true;
                result.Message = "OK";
                return result;
            }
            catch (Exception ex)
            {
                result.Message = ExceptionHelper.NotifyException("DeleteMany",
                    string.Concat("Some ", typeof(TEntity).Name, "s could not be deleted."), ex);
                return result;
            }
        }
        #endregion Delete

        #region Update
        public async Task<Result> UpdateOne(ObjectId id, UpdateDefinition<TEntity> update)
        {
            var filter = new FilterDefinitionBuilder<TEntity>().Eq("_id", id);
            return await UpdateOne(filter, update);
        }

        public async Task<Result> UpdateOne(FilterDefinition<TEntity> filter, UpdateDefinition<TEntity> update)
        {
            var result = new Result();
            try
            {
                var collection = GetCollection();
                var updateRes = await collection.UpdateOneAsync(filter, update);
                if (updateRes.ModifiedCount < 1)
                {
                    var ex = new Exception();
                    result.Message = ExceptionHelper.NotifyException("UpdateOne",
                        string.Concat("ERROR: updateRes.ModifiedCount < 1 for entity: ", typeof(TEntity).Name), ex);
                    return result;
                }
                result.Success = true;
                result.Message = "OK";
                return result;
            }
            catch (Exception ex)
            {
                result.Message = ExceptionHelper.NotifyException("UpdateOne",
                    string.Concat("Exception updating entity: ", typeof(TEntity).Name), ex);
                return result;
            }
        }

        public async Task<Result> UpdateMany(IEnumerable<ObjectId> ids, UpdateDefinition<TEntity> update)
        {
            var filter = new FilterDefinitionBuilder<TEntity>().In("_id", ids);
            return await UpdateOne(filter, update);
        }

        public async Task<Result> UpdateMany(FilterDefinition<TEntity> filter, UpdateDefinition<TEntity> update)
        {
            var result = new Result();
            try
            {
                var collection = GetCollection();
                var updateRes = await collection.UpdateManyAsync(filter, update);
                if (updateRes.ModifiedCount < 1)
                {
                    var ex = new Exception();
                    result.Message = ExceptionHelper.NotifyException("UpdateMany",
                        string.Concat("ERROR: updateRes.ModifiedCount < 1 for entities: ", typeof(TEntity).Name, "s"),
                        ex);
                    return result;
                }
                result.Success = true;
                result.Message = "OK";
                return result;
            }
            catch (Exception ex)
            {
                result.Message = ExceptionHelper.NotifyException("UpdateMany",
                    string.Concat("Exception updating entities: ", typeof(TEntity).Name, "s"), ex);
                return result;
            }
        }
        #endregion Update

        #region Find And Update

        public async Task<GetOneResult<TEntity>> GetAndUpdateOne(FilterDefinition<TEntity> filter,
            UpdateDefinition<TEntity> update, FindOneAndUpdateOptions<TEntity, TEntity> options)
        {
            var result = new GetOneResult<TEntity>();
            try
            {
                var collection = GetCollection();
                result.Entity = await collection.FindOneAndUpdateAsync(filter, update, options);
                result.Success = true;
                result.Message = "OK";
                return result;
            }
            catch (Exception ex)
            {
                result.Message = ExceptionHelper.NotifyException("GetAndUpdateOne",
                    string.Concat("Exception getting and updating entity: ", typeof(TEntity).Name), ex);
                return result;
            }
        }

        #endregion Find And Update
    }
}