using System.Reflection;
using Discord;
using Discord.Extensions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using UndyingBot.Core;
using UndyingBot.Discord.SlashCommands;

var services = new ServiceCollection();

// config
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddJsonFile("appsettings.Development.json", true)
    .AddJsonFile("appsettings.Secret.json", true)
    .Build();

services.AddSingleton(configuration);

// logs
var log = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();


Log.Logger = log;

services.AddLogging(builder =>
{
    builder.ClearProviders();
    builder.AddSerilog(log);
});

// setup slash commands
services.AddSlashCommands(Assembly.GetAssembly(typeof(QuoteCommand))!);

services.ConfigureCoreServices(configuration);


var client = new DiscordSocketClient();

client.Log += LogDiscord;
services.AddSingleton(client);

// build the provider
var provider = services.BuildServiceProvider();

//set up the db
await provider.MigrateUp(configuration);

// login and start the client
await client.LoginAsync(TokenType.Bot, configuration["DiscordToken"]);
await client.StartAsync();

client.Ready += async () =>
{
    // register slash commands when ready
    await provider.UseSlashCommands();
};

// wait forever
await Task.Delay(Timeout.Infinite);
return;

Task LogDiscord(LogMessage msg)
{
    const string messageTemplate = "{Message}";

    switch (msg.Severity)
    {
        case LogSeverity.Critical:
            log.Error(msg.Exception, messageTemplate, msg.Message);
            break;
        case LogSeverity.Error:
            log.Error(msg.Exception, messageTemplate, msg.Message);
            break;
        case LogSeverity.Warning:
            log.Warning(msg.Exception, messageTemplate, msg.Message);
            break;
        case LogSeverity.Info:
            log.Information(msg.Exception, messageTemplate, msg.Message);
            break;
        case LogSeverity.Verbose:
            log.Debug(msg.Exception, messageTemplate, msg.Message);
            break;
        case LogSeverity.Debug:
            log.Debug(msg.Exception, messageTemplate, msg.Message);
            break;
        default:
            throw new ArgumentOutOfRangeException();
    }

    return Task.CompletedTask;
}