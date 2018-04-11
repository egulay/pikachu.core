using System;
using MongoDB.Bson.Serialization.Attributes;
using Pikachu.Core.Repository.Mongo;

namespace CalculationTest.Mongo.Models
{
    [BsonIgnoreExtraElements]
    public class Account : MongoEntity
    {
        public string ShardKey { get; set; }
        public string Name { get; set; }
        public string CompanyName { get; set; }
        public DateTime Date { get; set; }
        public int TransactionNumber { get; set; }
        public double Value { get; set; }

    }
}
