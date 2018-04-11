using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CalculationTest.Mongo.Models.RateCalculation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Pikachu.Core.Repository;
using Pikachu.Core.Repository.Helpers;
using Result = CalculationTest.Mongo.Models.RateCalculation.Result;
using Account = CalculationTest.Mongo.Models.RateCalculation.Account;

namespace CalculationTest.Mongo
{
    public class CurrencyRatesConversionService : PikachuDataService
    {
        public CurrencyRatesConversionService()
        {
            CreateMongoDbServiceProvider(new ServiceCollection(),
                CreateDefaultConfiguration(Directory.GetCurrentDirectory(), "appsettings.json", false),
                "DatabaseConfiguration:RateConversionConnectionString", "DatabaseConfiguration:DatabaseName");

            CreateConsoleLoggerFor<CurrencyRatesConversionService>(LoggingLevel.Debug);
        }

        public async void SeedData()
        {
            try
            {
                var timer = Stopwatch.StartNew();

                ConsoleLogger.LogInformation("Seeding Account Data...");

                var currencyRates = Helpers.GenerateRates(new[] {"EUR", "USD", "GBP", "CZK", "PHP"}).ToArray();

                var rateResult = await MongoDataContext.Repository<Rate>().AddMany(currencyRates);

                if (!rateResult.Success)
                {
                    ConsoleLogger.LogError(rateResult.Message);
                    return;
                }

                ConsoleLogger.LogInformation("Currency Rates Created...");
                ConsoleLogger.LogInformation("Insert Loop Started...");
                
                Parallel.For(0, Enumerable.Range(0, 500).ToArray().Length, async i =>
                {
                    var chunk = new List<Account>();
                    var range = Helpers.AddAccountsForRateConversion(2000,
                        currencyRates.ToArray().Select(s => s.SourceCurrency));
                    chunk.AddRange(range);

                    var accountResult =
                        await MongoDataContext.LockedRepository<Account>().AddMany(chunk);

                    if (!accountResult.Success)
                    {
                        Console.WriteLine(string.Concat("Loop Step: ", i, " | Chunk Insert Error: ",
                            accountResult.Message));
                    }

                    chunk.Clear();
                });

                timer.Stop();
                var timespan = timer.Elapsed;
                Thread.Sleep(10000);
                ConsoleLogger.LogInformation(string.Concat("Seeding Account Data Finished. Duration: ",
                    string.Format("{0:00}:{1:00}:{2:00}", timespan.Minutes, timespan.Seconds, timespan.Milliseconds/10)));
                ConsoleLogger.LogInformation(string.Concat("Total Account Count: ",
                    MongoDataContext.Repository<Account>().GetCollection().AsQueryable().Count().ToString("N0")));
            }
            catch (Exception ex)
            {
                ConsoleLogger.LogCritical(ex.GetAllMessages());
            }
        }

        public async void GenerateConvertedRates()
        {
            try
            {
                ConsoleLogger.LogInformation(string.Concat("Total Account Count: ",
                    MongoDataContext.Repository<Account>().GetCollection().AsQueryable().Count().ToString("N0")));

                ConsoleLogger.LogInformation("Generating Calculated Values. Please wait...");
                var currencyRates = await MongoDataContext.Repository<Rate>().GetAll();
                var resultChunk = new List<Result>();
                var groupId = Guid.NewGuid();
                var generalTimer = Stopwatch.StartNew();
                const int pageSize = 5000;

                foreach (var rate in currencyRates.Entities)
                {
                    var currencyFilter = Builders<Account>.Filter.Eq(f => f.Currency,
                        rate.SourceCurrency);
                    var accountCount =
                        await MongoDataContext.Repository<Account>()
                            .Count(currencyFilter);

                    var executionTimer = Stopwatch.StartNew();
                    var numberOfPages = accountCount / pageSize + (accountCount % pageSize == 0 ? 0 : 1);
                    for (var i = 0; i < numberOfPages; i++)
                    {
                        var accountChunk =
                            await
                                MongoDataContext.Repository<Account>()
                                    .GetMany(currencyFilter, i * pageSize, pageSize);

                        resultChunk.AddRange(accountChunk.Entities.Select(account => new Result
                        {
                            Id = ObjectId.GenerateNewId(),
                            GroupId = groupId,
                            AccountName = account.Name,
                            CompanyName = account.Company,
                            GroupName = string.Concat("Query_", rate.SourceCurrency, "_to_", rate.TargetCurrency),
                            Value = account.Value * rate.Value
                        }));

                        var conversionResult = await MongoDataContext.Repository<Result>().AddMany(resultChunk);
                        if (!conversionResult.Success)
                            Console.WriteLine(conversionResult.Message);
                        resultChunk.Clear();

                    }
                    executionTimer.Stop();
                    var executionTimespan = executionTimer.Elapsed;
                    Console.WriteLine(string.Concat("Calculation completed for query: ", rate.SourceCurrency,
                        "_to_", rate.TargetCurrency,
                        " - Total acccount count for ", rate.SourceCurrency, " currency : ", accountCount.ToString("N0"),
                        " - Duration: ",
                        string.Format("{0:00}:{1:00}:{2:00}", executionTimespan.Minutes, executionTimespan.Seconds,
                            executionTimespan.Milliseconds / 10)));
                }

                generalTimer.Stop();
                var generalTimeSpan = generalTimer.Elapsed;

                ConsoleLogger.LogInformation(string.Concat("Total Calculated Account Count: ",
                    MongoDataContext.Repository<Result>().GetCollection().AsQueryable().Count().ToString("N0")));
                ConsoleLogger.LogInformation(string.Concat("Generate Converted Rates Finished. Total Duration: ",
                    string.Format("{0:00}:{1:00}:{2:00}", generalTimeSpan.Minutes, generalTimeSpan.Seconds,
                        generalTimeSpan.Milliseconds/10)));
            }
            catch (Exception ex)
            {
                ConsoleLogger.LogCritical(ex.GetAllMessages());
            }
        }

        public async void CloneCalculationCollection()
        {
            try
            {
                ConsoleLogger.LogInformation(string.Concat("Total Calculated Account Count: ",
                    MongoDataContext.Repository<Result>().GetCollection().AsQueryable().Count().ToString("N0")));

                ConsoleLogger.LogInformation("Cloning Calculation Results Collection. Please wait...");

                var generalTimer = Stopwatch.StartNew();

                var jsonScript = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(),
                    @"SampleMongoScripts\CloneResultsCollection.mongojs"));

                await MongoDataContext.Database.RunCommandAsync(new JsonCommand<BsonDocument>(jsonScript));

                generalTimer.Stop();
                var generalTimeSpan = generalTimer.Elapsed;
                ConsoleLogger.LogInformation(string.Concat("Clonned result count: ",
                    MongoDataContext.Repository<ResultClone>().GetCollection().AsQueryable().Count().ToString("N0")));
                ConsoleLogger.LogInformation(
                    string.Concat("Cloning Calculation Results Collection Finished. Total Duration: ",
                        string.Format("{0:00}:{1:00}:{2:00}", generalTimeSpan.Minutes, generalTimeSpan.Seconds,
                            generalTimeSpan.Milliseconds/10)));
            }
            catch (Exception ex)
            {
                ConsoleLogger.LogCritical(ex.GetAllMessages());
            }
        }
    }
}
