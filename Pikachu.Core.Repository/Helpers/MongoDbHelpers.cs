using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Pikachu.Core.Repository.Mongo;

namespace Pikachu.Core.Repository.Helpers
{
    public static class MongoDbHelpers
    {
        public static async Task<IEnumerable<TEntity>> GetCommandResultAsync<TEntity>(this IMongoDatabase database,
            string jsonScript) where TEntity : class, new()
        {
            var output = await database.RunCommandAsync(new JsonCommand<BsonDocument>(jsonScript));

            return BsonSerializer.Deserialize<MongoCommandResult<TEntity>>(output).Output;
        }

        private static string FormatJson(this string json)
        {
            const string indentString = "    ";

            var indentation = 0;
            var quoteCount = 0;
            var result =
                from ch in json
                let quotes = ch == '"' ? quoteCount++ : quoteCount
                let lineBreak =
                ch == ',' && quotes%2 == 0
                    ? string.Concat(ch, Environment.NewLine, string.Concat(Enumerable.Repeat(indentString, indentation)))
                    : null
                let openChar =
                ch == '{' || ch == '['
                    ? string.Concat(ch, Environment.NewLine,
                        string.Concat(Enumerable.Repeat(indentString, ++indentation)))
                    : ch.ToString()
                let closeChar =
                ch == '}' || ch == ']'
                    ? string.Concat(Environment.NewLine, string.Concat(Enumerable.Repeat(indentString, --indentation)),
                        ch)
                    : ch.ToString()
                select lineBreak ?? (openChar.Length > 1
                           ? openChar
                           : closeChar);

            return string.Concat(result);
        }
    }
}
