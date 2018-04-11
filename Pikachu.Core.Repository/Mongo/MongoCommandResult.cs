using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace Pikachu.Core.Repository.Mongo
{
    public class MongoCommandResult<TResult> where TResult : class, new()
    {
        [BsonElement("waitedMS")]
        public int WaitedMs { get; set; }
        [BsonElement("result")]
        public IEnumerable<TResult> Output { get; set; }
        [BsonElement("ok")]
        public int IsOk { get; set; }
    }
}
