using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using Xunit;

namespace EFCore.OpenEdge.Tests.TestUtilities
{
    public abstract class OpenEdgeTestBase : IDisposable
    {
        protected IConfiguration Configuration { get; }
        protected ServiceProvider ServiceProvider { get; }
        protected string ConnectionString { get; }
        
        private readonly ILoggerFactory _loggerFactory;
        
        protected OpenEdgeTestBase()
        {
            Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            ConnectionString = Configuration.GetConnectionString("OpenEdgeConnection");
            
            _loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            
            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();
        }

        protected void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(Configuration);
        }

        protected DbContextOptionsBuilder<T> CreateOptionsBuilder<T>() where T : DbContext
        {
            return new DbContextOptionsBuilder<T>()
                .UseOpenEdge(ConnectionString)
                .EnableSensitiveDataLogging()
                .UseLoggerFactory(_loggerFactory);
        }

        protected DbContextOptions CreateOptions()
        {
            return new DbContextOptionsBuilder()
                .UseOpenEdge(ConnectionString)
                .EnableSensitiveDataLogging()
                .UseLoggerFactory(_loggerFactory)
                .Options;
        }

        /// <summary>
        /// Tests basic connection functionality similar to your example
        /// </summary>
        protected void TestBasicConnection()
        {
            // Arrange
            var contextOptions = CreateOptions();
                
            using (var context = new OpenEdgeContext(contextOptions))
            {
                // Act
                var connection = context.Database.GetDbConnection();
                connection.Open();
                
                // Assert
                Assert.NotNull(connection.Database);
                Assert.Equal(System.Data.ConnectionState.Open, connection.State);
                
                connection.Close();
            }
        }

        public virtual void Dispose()
        {
            ServiceProvider?.Dispose();
        }
    }
}