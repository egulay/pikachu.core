using MongoDB.Bson;

namespace Pikachu.Core.Repository.Mongo.Interfaces
{
    public interface IMongoEntity
    {
        ObjectId Id { get; set; }
    }
}