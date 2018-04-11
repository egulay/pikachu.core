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
    public class CalculatorService : PikachuDataService, ICalculatorService
    {
        private readonly int _testCount;
        private int _testCounter;

        public CalculatorService()
        {
            try
            {
                CreateMongoDbServiceProvider(new ServiceCollection(),
                    CreateDefaultConfiguration(Directory.GetCurrentDirectory(), "appsettings.json", false),
                    "DatabaseConfiguration:ConnectionString", "DatabaseConfiguration:DatabaseName");

                CreateConsoleLoggerFor<CalculatorService>(LoggingLevel.Debug);

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
                await Task.Run(() =>
                {
                    ConsoleLogger.LogInformation(
                        string.Concat("Calculation Test: Get single account by Id: ",
                            id, " Started."));

                    var timer = Stopwatch.StartNew();

                    var query = (from account
                        in MongoDataContext.Repository<Account>().GetCollection().AsQueryable()
                        where account.Id == id
                        join factor
                        in MongoDataContext.Repository<AccountFactor>().GetCollection().AsQueryable()
                        on account.Id equals factor.AccountId into joined
                        select new {account, joined}).ToList().Select(s => new SingleSearchResult
                    {
                        CompanyName = s.account.CompanyName,
                        Name = s.account.Name,
                        Factor = s.joined.FirstOrDefault().Factor,
                        Value = s.account.Value,
                        Total = s.account.Value*s.joined.FirstOrDefault().Factor
                    });

                    timer.Stop();

                    Console.WriteLine(string.Concat(Environment.NewLine, "### GetSingleAccountById >> ", id,
                        " ###"));
                    ConsoleTable.From(query).Write(Format.Alternative);

                    var timespan = timer.Elapsed;
                    ConsoleLogger.LogInformation(string.Concat("Calculation Test: Get single account by Id: ",
                        id, " Finished. Duration: ",
                        string.Format("{0:00}:{1:00}:{2:00}", timespan.Minutes, timespan.Seconds,
                            timespan.Milliseconds/10)));

                    _testCounter++;
                    ConsoleLogger.LogWarning(string.Concat("Test(s) left: ", _testCounter, "/", _testCount));
                });
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
                    string.Concat("Calculation Test: Lookup Group By Account for Company: ",
                        companyName, " Started."));

                var script =
                    File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(),
                            @"SampleMongoScripts\AggregateMongoAccounts.mongojs"))
                        .Replace("[[CompanyNameParam]]", companyName);

                var timer = Stopwatch.StartNew();

                var result = await MongoDataContext.Database.GetCommandResultAsync<CalculationResult>(script);

                timer.Stop();

                Console.WriteLine(string.Concat(Environment.NewLine, "### LookupGroupByAccountFromCommand >> ",
                    companyName, " ###"));
                ConsoleTable.From(result).Write(Format.Alternative);

                var timespan = timer.Elapsed;
                ConsoleLogger.LogInformation(
                    string.Concat("Calculation Test: Lookup Group By Account for Company: ",
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

                foreach (var i in Enumerable.Range(0, 75000).ToArray())
                {
                    var accountCount = new Random().Next(1, 10);
                    var companyName = Helpers.GetRandomString("C", 1, 10);
                    if (chunk.Count <= 7500)
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
