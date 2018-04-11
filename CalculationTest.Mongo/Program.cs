using MongoDB.Bson;

namespace CalculationTest.Mongo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            System.Console.WriteLine("### Calculation Test on MongoDB Started ###");

            //ExecuteTests();

            //ExecuteClusteredTests();

            ExecuteCurrencyConversionTest();

            System.Console.ReadLine();
        }


        public static void ExecuteCurrencyConversionTest()
        {
            var service = new CurrencyRatesConversionService();

            //service.SeedData();

            service.GenerateConvertedRates();

            //service.CloneCalculationCollection();
        }

        public static void ExecuteTests()
        {
            var calculationService =
                CalculatorServiceFactory.Create(CalculatorServiceType.Standard);

            //calculationService.SeedAccountData();

            //calculationService.WriteDataCounts();

            //calculationService.GetSingleAccountById(ObjectId.Parse("5875d5766ed0fc395c2bae71"));

            //calculationService.GroupByAccountNameWithSumValue();

            //calculationService.GroupByAccountNameWithSumValue("C1");

            calculationService.GroupByAccountFromCommand("C1");
        }

        public static void ExecuteClusteredTests()
        {
            var calculationService =
                CalculatorServiceFactory.Create(CalculatorServiceType.Clustered);

            //calculationSerice.SeedAccountData();

            calculationService.WriteDataCounts();

            calculationService.GetSingleAccountById(ObjectId.Parse("5857e825b5eed806dc7fbf4d"));

            calculationService.GroupByAccountNameWithSumValue();

            calculationService.GroupByAccountNameWithSumValue("C2");

            calculationService.GroupByAccountFromCommand("C1");
        }
    }
}
