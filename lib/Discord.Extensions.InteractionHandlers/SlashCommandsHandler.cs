using Discord.Extensions.InteractionHandlers.Abstractions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Discord.Extensions.InteractionHandlers;

public class SlashCommandsHandler(
    IServiceProvider provider,
    DiscordSocketClient client,
    ILogger<SlashCommandsHandler> logger
) : ISlashCommandHandler
{
    public async ValueTask RegisterAsync()
    {
        await using var scope = provider.CreateAsyncScope();
        var commands = scope.ServiceProvider.GetServices<ISlashCommand>();

        int count = 0;

        foreach (var command in commands)
        {
            logger.LogDebug("Registering command {CommandName}", command.Name);
            try
            {
                var builder = new SlashCommandBuilder()
                    .WithName(command.Name)
                    .WithDescription(command.Description);
                var res = await command.RegisterAsync(builder);
                await client.CreateGlobalApplicationCommandAsync(res.Build());
                count++;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to register command {CommandName}", command.Name);
            }
        }

        logger.LogInformation("Registered {Count} slash commands", count);
    }

    public async Task HandleAsync(SocketSlashCommand command)
    {
        try
        {
            await using var scope = provider.CreateAsyncScope();
            var commands = scope.ServiceProvider.GetServices<ISlashCommand>();
            var commandToHandle = commands.FirstOrDefault(x => x.Name == command.Data.Name);
            if (commandToHandle is null)
            {
                logger.LogError("Command {CommandName} can't be handled", command.Data.Name);
                await command.RespondAsync(
                    "Command not found. If you think this is an error, please contact the bot owner.");
                return;
            }

            await commandToHandle.HandleAsync(command);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to handle command {CommandName}", command.Data.Name);
            await command.RespondAsync("An error occurred while handling the command. Please try again later.",
                ephemeral: true);
        }
    }
}