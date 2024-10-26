using System.Data;
using FluentMigrator.Runner;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UndyingBot.Core.Db;
using UndyingBot.Core.Options;
using UndyingBot.Core.Services;

namespace UndyingBot.Core;

public static class DependencyInjection
{
    public static void ConfigureCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<QuoteOptions>(configuration.GetSection("Quotes"));
        services.AddSingleton<QuoteService>();

        var connectionString = configuration.GetConnectionString("Sqlite");
        if (connectionString is null)
        {
            throw new Exception("No db defined!");
        }
        services.AddTransient<SqliteConnection>(_ => new(connectionString));
        services.AddScoped<IDbConnection>(_ => new SqliteConnection(connectionString));
        services.AddSingleton<SqliteConnectionFactory>(_ => new(connectionString));
        
        services.AddFluentMigratorCore()
            .ConfigureRunner(rb =>
                rb.AddSQLite()
                    .WithGlobalConnectionString(connectionString)
                    .ScanIn(typeof(Db.Migrations.Init).Assembly).For.Migrations())
            .AddLogging(lb => lb.AddFluentMigratorConsole());
    }
    
    public static async Task MigrateUp(this IServiceProvider provider, IConfiguration configuration)
    {
        await using var scope = provider.CreateAsyncScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
    }
}