using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using Microsoft.Data.Sqlite;

namespace Kallipr_IOT_Monitor_Backend.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IDisposable
{
    private readonly string _testDbPath = Path.Combine(Path.GetTempPath(), $"test_db_{Guid.NewGuid()}.db");
    private readonly string _connectionString;

    public CustomWebApplicationFactory()
    {
        _connectionString = $"Data Source={_testDbPath}";
        TestDatabaseInitializer.Initialize(_connectionString);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ConnectionStrings:KalliprDb"] = _connectionString
            }!);
        });

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IDbConnection));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddScoped<IDbConnection>(sp =>
            {
                var connection = new SqliteConnection(_connectionString);
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "PRAGMA foreign_keys = ON;";
                    command.ExecuteNonQuery();
                }

                return connection;
            });
        });
    }

    public new void Dispose()
    {
        if (File.Exists(_testDbPath))
        {
            try
            {
                Thread.Sleep(100);
                File.Delete(_testDbPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not delete test database: {ex.Message}");
            }
        }

        base.Dispose();
        GC.SuppressFinalize(this);
    }
}