using MongoDB.Bson;

namespace CalculationTest.Mongo
{
    public interface ICalculatorService
    {
        void WriteDataCounts();
        void GetSingleAccountById(ObjectId id);
        void GroupByAccountFromCommand(string companyName);
        void GroupByAccountNameWithSumValue();
        void GroupByAccountNameWithSumValue(string companyName);
        void SeedAccountData();
    }
}