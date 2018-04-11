using System.Collections.Generic;
using Pikachu.Core.Repository.Mongo.Interfaces;

namespace Pikachu.Core.Repository.Mongo
{
    public class Result
    {
        public bool Success { get; internal set; }
        public string Message { get; internal set; }
        public int ErrorCode { get; internal set; }
        public Result()
        {
            Success = false;
            Message = "";
            ErrorCode = 500;
        }
    }

    public class GetOneResult<TEntity> : Result where TEntity : IMongoEntity
    {
        public TEntity Entity { get; set; }
    }

    public class GetManyResult<TEntity> : Result where TEntity : IMongoEntity
    {
        public IEnumerable<TEntity> Entities { get; set; }
        public int TotalCount { get; set; }
    }

    public class GetListResult<TEntity> : Result where TEntity : IMongoEntity
    {
        public IList<TEntity> Entities { get; set; }
        public int TotalCount { get; set; }
    }
}
