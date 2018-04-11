using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Pikachu.Core.Repository.Mongo;

namespace CalculationTest.Mongo.Models
{
    [BsonIgnoreExtraElements]
    public class AccountFactor : MongoEntity
    {
        public string ShardKey { get; set; }
        [BsonElement("account_id")]
        public ObjectId AccountId { get; set; }
        public DateTime Date { get; set; }
        public double Factor { get; set; }
    }
}
