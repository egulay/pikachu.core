using MongoDB.Bson.Serialization.Attributes;
using Pikachu.Core.Repository.Mongo;

namespace CalculationTest.Mongo.Models.RateCalculation
{
    [BsonIgnoreExtraElements]
    public class Rate : MongoEntity
    {
        public string SourceCurrency { get; set; }
        public string TargetCurrency { get; set; }
        public double Value { get; set; }
    }
}
