using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Pikachu.Core.Repository.Mongo.Interfaces;

namespace Pikachu.Core.Repository.Mongo
{
    public class MongoEntity : IMongoEntity
    {
        [BsonId]
        public ObjectId Id { get; set; }
    }
}
