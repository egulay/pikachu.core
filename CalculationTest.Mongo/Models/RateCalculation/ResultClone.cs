using System;
using MongoDB.Bson.Serialization.Attributes;
using Pikachu.Core.Repository.Mongo;

namespace CalculationTest.Mongo.Models.RateCalculation
{
    [BsonIgnoreExtraElements]
    public class ResultClone : MongoEntity
    {
        public Guid GroupId { get; set; }
        public string GroupName { get; set; }
        public string AccountName { get; set; }
        public string CompanyName { get; set; }
        public double Value { get; set; }
    }
}
