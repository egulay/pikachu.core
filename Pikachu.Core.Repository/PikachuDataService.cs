using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pikachu.Core.Repository.Helpers;
using Pikachu.Core.Repository.Mongo;
using Pikachu.Core.Repository.Mongo.Interfaces;
using Pikachu.Core.Repository.Sql;
using Pikachu.Core.Repository.Sql.Interfaces;

namespace Pikachu.Core.Repository
{
    public abstract class PikachuDataService
    {
        protected enum LoggingLevel : short
        {
            Debug,
            Trace
        }

        protected IUnitOfWork SqlDataContext { get; private set; }
        protected IMongoWorker MongoDataContext { get; private set; }
        protected ILogger ConsoleLogger { get; private set; }

        protected ILogger CreateConsoleLoggerFor<TService>(LoggingLevel loggingLevel) where TService : class, new()
        {
            try
            {
                ConsoleLogger = new LoggerFactory().WithFilter(new FilterLoggerSettings
                    {
                        {"Microsoft", LogLevel.Warning},
                        {"System", LogLevel.Warning},
                        {typeof(TService).Name, loggingLevel == LoggingLevel.Debug ? LogLevel.Debug : LogLevel.Trace}
                    })
                    .AddConsole()
                    .CreateLogger(typeof(TService));

                return ConsoleLogger;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Concat("Console logger initialization exception(s):", Environment.NewLine,
                    ex.GetAllMessages()));
            }
        }

        protected IServiceProvider CreateSqlServiceProvider<TContext>(IServiceCollection serviceCollection,
            IConfigurationRoot configuration, string connectionStringKeyInConfiguration)
            where TContext : DbContext, IDbContext
        {
            try
            {
                serviceCollection.AddTransient<IUnitOfWork, UnitOfWork>();
                serviceCollection.Add(new ServiceDescriptor(typeof(IDbContext), typeof(TContext),
                    ServiceLifetime.Transient));

                serviceCollection.AddDbContext<TContext>(
                    options => options.UseSqlServer(configuration[connectionStringKeyInConfiguration]));

                var serviceProvider = serviceCollection.BuildServiceProvider();

                SqlDataContext = serviceProvider.GetService<IUnitOfWork>();

                return serviceProvider;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Concat("Create Sql Service Provider Initialization Exception(s):",
                    Environment.NewLine, ex.GetAllMessages()));
            }
        }

        protected IServiceProvider CreateMongoDbServiceProvider(IServiceCollection serviceCollection,
            IConfigurationRoot configuration, string connectionStringKeyInConfiguration,
            string databaseNameKeyInConfiguration)
        {
            try
            {
                serviceCollection.AddTransient<IMongoWorker, MongoWorker>();
                serviceCollection.AddSingleton<IMongoDbContext>(
                    provider =>
                        new MongoDbContext(configuration[connectionStringKeyInConfiguration],
                            configuration[databaseNameKeyInConfiguration]));

                var serviceProvider = serviceCollection.BuildServiceProvider();

                MongoDataContext = serviceProvider.GetService<IMongoWorker>();

                return serviceProvider;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Concat("Create Mongo Service Provider Initialization Exception(s):",
                    Environment.NewLine, ex.GetAllMessages()));
            }
        }

        protected IConfigurationRoot CreateDefaultConfiguration(string basePath, string jsonFilePath, bool isOptional)
        {
            try
            {
                return new ConfigurationBuilder()
                    .SetBasePath(basePath)
                    .AddJsonFile(jsonFilePath, isOptional)
                    .Build();
            }
            catch (Exception ex)
            {
                throw new Exception(string.Concat("Create Default Configuration Initialization Exception(s):",
                    Environment.NewLine, ex.GetAllMessages()));
            }
        }
    }
}

