using MongoDB.Bson.Serialization.Attributes;

namespace CalculationTest.Mongo.Models
{
    public class CalculationResult
    {
        [BsonElement("_id")]
        public string AccountName { get; set; }
        public double Total { get; set; }
    }

    public class SingleSearchResult
    {
        public string Name { get; set; }
        public string CompanyName { get; set; }
        public double Value { get; set; }
        public double Factor { get; set; }
        public double Total { get; set; }
    }

    public class MapReduceCalculationResult
    {
        [BsonElement("_id")]
        public string AccountName { get; set; }
        [BsonElement("grandTotal")]
        public double GrandTotal { get; set; }
    }
}
