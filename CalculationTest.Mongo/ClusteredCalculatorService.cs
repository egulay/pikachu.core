using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CalculationTest.Mongo.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Pikachu.Core.Repository;
using Pikachu.Core.Repository.Helpers;

namespace CalculationTest.Mongo
{
    public class ClusteredCalculatorService : PikachuDataService, ICalculatorService
    {
        private readonly int _testCount;
        private int _testCounter;

        public ClusteredCalculatorService()
        {
            try
            {
                CreateMongoDbServiceProvider(new ServiceCollection(),
                    CreateDefaultConfiguration(Directory.GetCurrentDirectory(), "appsettings.json", false),
                    "DatabaseConfiguration:ShardedClusterConnectionString", "DatabaseConfiguration:DatabaseName");

                CreateConsoleLoggerFor<ClusteredCalculatorService>(LoggingLevel.Debug);

                _testCount = 4;
                _testCounter = 0;
            }
            catch (Exception ex)
            {
                ConsoleLogger.LogCritical(ex.GetAllMessages());
            }
        }

        public void WriteDataCounts()
        {
            try
            {
                var accountCount =
                    MongoDataContext.Repository<Account>().GetCollection().AsQueryable().Count().ToString("N0");

                var accountFactorCount =
                    MongoDataContext.Repository<AccountFactor>().GetCollection().AsQueryable().Count().ToString("N0");

                Console.WriteLine(string.Concat("Accounts Document Count: ", accountCount));

                Console.WriteLine(string.Concat("Account Factors Document Count: ", accountFactorCount));

                Console.WriteLine(string.Concat("Total: ", accountCount + accountFactorCount));
            }
            catch (Exception ex)
            {
                ConsoleLogger.LogCritical(ex.GetAllMessages());
            }
        }

        public async void GetSingleAccountById(ObjectId id)
        {
            try
            {
                ConsoleLogger.LogInformation(
                    string.Concat("Calculation Test: Get single account by Id: ",
                        id, " Started."));

                var timer = Stopwatch.StartNew();

                var accountEntry = await MongoDataContext.Repository<Account>().GetOne(id);

                if (!accountEntry.Success)
                {
                    ConsoleLogger.LogWarning(accountEntry.Message);
                    timer.Stop();
                    return;
                }
                var factorEntry =
                    await
                        MongoDataContext.Repository<AccountFactor>()
                            .GetOne(Builders<AccountFactor>.Filter.Eq(f => f.AccountId, accountEntry.Entity.Id));

                if (!factorEntry.Success)
                {
                    ConsoleLogger.LogWarning(factorEntry.Message);
                    timer.Stop();
                    return;
                }

                var output = new SingleSearchResult
                {
                    CompanyName = accountEntry.Entity.CompanyName,
                    Name = accountEntry.Entity.Name,
                    Factor = factorEntry.Entity.Factor,
                    Value = accountEntry.Entity.Value,
                    Total = accountEntry.Entity.Value*factorEntry.Entity.Factor
                };

                timer.Stop();

                Console.WriteLine(string.Concat(Environment.NewLine, "### GetSingleAccountById >> ", id, " ###"));
                ConsoleTable.From(new[] {output}).Write(Format.Alternative);

                var timespan = timer.Elapsed;
                ConsoleLogger.LogInformation(string.Concat("Calculation Test: Get single account by Id: ",
                    id, " Finished. Duration: ",
                    string.Format("{0:00}:{1:00}:{2:00}", timespan.Minutes, timespan.Seconds,
                        timespan.Milliseconds/10)));

                _testCounter++;
                ConsoleLogger.LogWarning(string.Concat("Test(s) left: ", _testCounter, "/", _testCount));
            }
            catch (Exception ex)
            {
                ConsoleLogger.LogCritical(ex.GetAllMessages());
            }
        }

        public async void GroupByAccountFromCommand(string companyName)
        {
            try
            {
                ConsoleLogger.LogInformation(
                    string.Concat("Calculation Test: Mapreduce on Sharded Cluster: ",
                        companyName, " Started."));

                const string reduceCollectionName = "calculationresults";

                var accountMapScript = Helpers.GetScriptContent(@"SampleMongoScripts\MapReduce\accountMap.mongojs");
                var accountFactorMapScript =
                    Helpers.GetScriptContent(@"SampleMongoScripts\MapReduce\accountFactorMap.mongojs");
                var reduceScript = Helpers.GetScriptContent(@"SampleMongoScripts\MapReduce\reduce.mongojs");
                var finalizeScript = Helpers.GetScriptContent(@"SampleMongoScripts\MapReduce\finalize.mongojs");

                var mapReduceAggregationScript =
                    File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(),
                            @"SampleMongoScripts\MapReduce\aggregation.mongojs"))
                        .Replace("[[TargetCollection]]", reduceCollectionName)
                        .Replace("[[CompanyName]]", companyName);


                var timer = Stopwatch.StartNew();

                Task.Run(async () =>
                {
                    await MongoDataContext.Repository<Account>()
                        .GetCollection()
                        .MapReduceAsync(accountMapScript, reduceScript,
                            new MapReduceOptions<Account, BsonDocument>
                            {
                                Finalize = finalizeScript,
                                OutputOptions =
                                    MapReduceOutputOptions.Reduce(reduceCollectionName,
                                        MongoDataContext.Database.DatabaseNamespace.DatabaseName)
                            });

                    await MongoDataContext.Repository<AccountFactor>()
                        .GetCollection()
                        .MapReduceAsync(accountFactorMapScript, reduceScript,
                            new MapReduceOptions<AccountFactor, BsonDocument>
                            {
                                Finalize = finalizeScript,
                                OutputOptions =
                                    MapReduceOutputOptions.Reduce(reduceCollectionName,
                                        MongoDataContext.Database.DatabaseNamespace.DatabaseName)
                            });
                    ConsoleLogger.LogWarning(string.Concat("Mapreduce finished. New collection created: ",
                        reduceCollectionName));
                }).Wait();

                ConsoleLogger.LogWarning(string.Concat("Document count for ", reduceCollectionName, ": ",
                    MongoDataContext.Database.GetCollection<BsonDocument>(reduceCollectionName)
                        .AsQueryable()
                        .Count()
                        .ToString("N0")));

                var result =
                    await
                        MongoDataContext.Database.GetCommandResultAsync<MapReduceCalculationResult>(
                            mapReduceAggregationScript);

                timer.Stop();

                Console.WriteLine(string.Concat(Environment.NewLine, "### MapreduceOnShardedCluster >> ",
                    companyName, " ###"));
                ConsoleTable.From(result).Write(Format.Alternative);

                var timespan = timer.Elapsed;
                ConsoleLogger.LogInformation(
                    string.Concat("Calculation Test: Mapreduce on Sharded Cluster: ",
                        companyName, " Finished. Duration: ",
                        string.Format("{0:00}:{1:00}:{2:00}", timespan.Minutes, timespan.Seconds,
                            timespan.Milliseconds/10)));

                _testCounter++;
                ConsoleLogger.LogWarning(string.Concat("Test(s) left: ", _testCounter, "/", _testCount));
            }
            catch (Exception ex)
            {
                ConsoleLogger.LogCritical(ex.GetAllMessages());
            }
        }

        public async void GroupByAccountNameWithSumValue()
        {
            try
            {
                ConsoleLogger.LogInformation("Calculation Test: Group By Account Name With Sum on Value Started.");
                var timer = Stopwatch.StartNew();

                var aggregation =
                    await MongoDataContext.Repository<Account>().GetCollection().Aggregate()
                        .Group(r => new {r.Name}
                            , g => new {Account = g.Key, Total = g.Sum(f => f.Value)})
                        .ToListAsync();

                timer.Stop();

                Console.WriteLine(Environment.NewLine, "### GroupByAccountNameWithSumValue ###");
                ConsoleTable.From(aggregation).Write(Format.Alternative);

                var timespan = timer.Elapsed;
                ConsoleLogger.LogInformation(string.Concat("Group By Account Name With Sum Value Finished. Duration: ",
                    string.Format("{0:00}:{1:00}:{2:00}", timespan.Minutes, timespan.Seconds, timespan.Milliseconds/10)));

                _testCounter++;
                ConsoleLogger.LogWarning(string.Concat("Test(s) left: ", _testCounter, "/", _testCount));
            }
            catch (Exception ex)
            {
                ConsoleLogger.LogCritical(ex.GetAllMessages());
            }
        }

        public async void GroupByAccountNameWithSumValue(string companyName)
        {
            try
            {
                ConsoleLogger.LogInformation(
                    string.Concat("Calculation Test: Group By Account Name With Sum on Value for Company: ",
                        companyName, " Started."));

                var timer = Stopwatch.StartNew();

                var aggregation =
                    await MongoDataContext.Repository<Account>().GetCollection().Aggregate()
                        .Match(Builders<Account>.Filter.Eq(a => a.CompanyName, companyName))
                        .Group(r => new {r.Name}
                            , g => new {Account = g.Key, Total = g.Sum(f => f.Value)})
                        .ToListAsync();

                timer.Stop();

                Console.WriteLine(string.Concat(Environment.NewLine, "### GroupByAccountNameWithSumValue >> ",
                    companyName, " ###"));
                ConsoleTable.From(aggregation).Write(Format.Alternative);

                var timespan = timer.Elapsed;
                ConsoleLogger.LogInformation(
                    string.Concat("Calculation Test: Group By Account Name With Sum on Value for Company: ",
                        companyName, " Finished. Duration: ",
                        string.Format("{0:00}:{1:00}:{2:00}", timespan.Minutes, timespan.Seconds,
                            timespan.Milliseconds/10)));

                _testCounter++;
                ConsoleLogger.LogWarning(string.Concat("Test(s) left: ", _testCounter, "/", _testCount));
            }
            catch (Exception ex)
            {
                ConsoleLogger.LogCritical(ex.GetAllMessages());
            }
        }

        public async void SeedAccountData()
        {
            try
            {
                var timer = Stopwatch.StartNew();

                ConsoleLogger.LogInformation("Seeding Account Data...");
                var chunk = new List<Account>();

                foreach (var i in Enumerable.Range(0, 100000).ToArray())
                {
                    var accountCount = new Random().Next(1, 10);
                    var companyName = Helpers.GetRandomString("C", 1, 10);
                    if (chunk.Count <= 1000)
                        chunk.AddRange(Helpers.AddAccountsToChunk(accountCount, companyName));
                    else
                    {
                        var accountResult = await MongoDataContext.Repository<Account>().AddMany(chunk);

                        if (accountResult.Success)
                        {
                            ConsoleLogger.LogInformation(string.Concat("Loop Step: ", i, " | Chunk insert: ",
                                accountResult.Message));

                            var factorResults =
                                await MongoDataContext.Repository<AccountFactor>().AddMany(Helpers.AddToFactors(chunk));

                            if (factorResults.Success)
                                ConsoleLogger.LogInformation(string.Concat("Factor datas inserted for loop step ", i));
                            else
                                ConsoleLogger.LogError(factorResults.Message);
                        }
                        else
                            ConsoleLogger.LogError(accountResult.Message);

                        chunk.Clear();
                    }
                }

                timer.Stop();
                var timespan = timer.Elapsed;

                WriteDataCounts();
                ConsoleLogger.LogInformation(string.Concat("Seeding Account Data Finished. Duration: ",
                    string.Format("{0:00}:{1:00}:{2:00}", timespan.Minutes, timespan.Seconds, timespan.Milliseconds/10)));
            }
            catch (Exception ex)
            {
                ConsoleLogger.LogCritical(ex.GetAllMessages());
            }
        }
    }
}
