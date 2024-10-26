using Discord;
using Discord.WebSocket;

namespace UndyingBot.Discord;

public class DiscordBot(ILogger<DiscordBot> logger, IConfiguration configuration, DiscordSocketClient client) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var token = configuration["DiscordToken"];
        
        logger.LogInformation("Starting Discord bot");
        await client.LoginAsync(TokenType.Bot, token);
        
        await client.StartAsync();
        
        await Task.Delay(-1, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping Discord bot");
        return Task.CompletedTask;
    }

   
}