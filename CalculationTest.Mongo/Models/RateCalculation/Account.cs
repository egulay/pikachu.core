using MongoDB.Bson.Serialization.Attributes;
using Pikachu.Core.Repository.Mongo;

namespace CalculationTest.Mongo.Models.RateCalculation
{
    [BsonIgnoreExtraElements]
    public class Account : MongoEntity
    {
        public string Name { get; set; }
        public string Company { get; set; }
        public string Currency { get; set; }
        public int TransactionNumber { get; set; }
        public double Value { get; set; }
    }
}
