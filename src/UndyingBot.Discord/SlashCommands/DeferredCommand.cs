using Discord;
using Discord.Extensions.InteractionHandlers;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace UndyingBot.Discord.SlashCommands;

public class DeferredCommand(ILogger<DeferredCommand> logger) : SlashCommand
{
    public override string Name => "def";
    public override string Description => "testing deferred commands";
    
    public override ValueTask<SlashCommandBuilder> RegisterAsync(SlashCommandBuilder builder)
    {
        builder
            .AddOption("time", ApplicationCommandOptionType.Integer, "Time to wait", isRequired:false, minValue:0, maxValue:100);
        return ValueTask.FromResult(builder);
    }
    public override async Task HandleAsync(SocketSlashCommand command)
    {
        long time = command.Data.Options.FirstOrDefault(x => x.Name == "time")?.Value as long? ?? 5;
        await command.RespondAsync($"Это сообщение поменяется через {time} секунд!", ephemeral: false);
        logger.LogInformation("Delaying for {Time} seconds", time);
        await Task.Delay(TimeSpan.FromSeconds(time));
        await command.ModifyOriginalResponseAsync(properties =>
        {
            properties.Content = $"{time} секунд прошло!";
        });
    }
}