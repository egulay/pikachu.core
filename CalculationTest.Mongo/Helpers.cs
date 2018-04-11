using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CalculationTest.Mongo.Models;
using CalculationTest.Mongo.Models.RateCalculation;
using MongoDB.Bson;
using Account = CalculationTest.Mongo.Models.Account;

namespace CalculationTest.Mongo
{
    public static class Helpers
    {
        public static string GetRandomString(string letter, int start, int limit)
        {
            return string.Concat(letter, new Random().Next(start, limit).ToString());
        }

        public static double GetRandomDouble(double minValue, double maxValue)
        {
            return minValue + new Random().NextDouble()*(maxValue - minValue);
        }

        public static DateTime GetRandomDate(DateTime from, DateTime to)
        {
            return from + new TimeSpan((long) (new Random().NextDouble()*(to - from).Ticks));
        }

        public static IEnumerable<Account> AddAccountsToChunk(int count, string companyName)
        {
            return Enumerable.Range(0, count).Select(a => new Account
            {
                Id = ObjectId.GenerateNewId(),
                ShardKey = companyName, //Guid.NewGuid().ToString(),
                Value = GetRandomDouble(1000.00, 50000.00),
                CompanyName = companyName, //Helpers.GetRandomString("C", 1, 20),
                Date = GetRandomDate(DateTime.Parse("01.01.2016"), DateTime.Parse("31.12.2016")),
                Name = GetRandomString("A", 1, 10),
                TransactionNumber = a
            }).AsParallel();
        }

        public static IEnumerable<Rate> GenerateRates(IEnumerable<string> currencyList)
        {
            var currencies = currencyList as string[] ?? currencyList.ToArray();

            return (from currency in currencies
                let indiceExcluded = currencies.Where(c => !c.Equals(currency))
                from item in indiceExcluded
                select new Rate
                {
                    Id = ObjectId.GenerateNewId(),
                    SourceCurrency = currency,
                    TargetCurrency = item,
                    Value = GetRandomDouble(1.5, 4.0)
                }).ToList();
        }

        public static IEnumerable<Models.RateCalculation.Account> AddAccountsForRateConversion(int count,
            IEnumerable<string> currencyList)
        {
            return Enumerable.Range(0, count).Select(a =>
            {
                var currencies = currencyList as string[] ?? currencyList.ToArray();
                return new Models.RateCalculation.Account
                {
                    Id = ObjectId.GenerateNewId(),
                    Company = GetRandomString("C", 1, 101),
                    Name = GetRandomString("A", 1, 10001),
                    Currency = currencies.ToArray()[new Random().Next(currencies.ToArray().Length - 1)],
                    Value = GetRandomDouble(1000, 5000),
                    TransactionNumber = new Random().Next(1, 51)
                };
            }).AsParallel();
        }

        public static IEnumerable<AccountFactor> AddToFactors(IEnumerable<Account> accounts)
        {
            return accounts.Select(account => new AccountFactor
            {
                Id = ObjectId.GenerateNewId(),
                ShardKey = account.ShardKey, //account.CompanyName, //Guid.NewGuid().ToString(),
                AccountId = account.Id,
                Date = account.Date,
                Factor = GetRandomDouble(1.00, 50.00)
            }).AsParallel();
        }

        public static BsonJavaScript GetScriptContent(string localPath)
        {
            return new BsonJavaScript(File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(),
                localPath)));
        }
    }
}
