using Discord.Extensions.InteractionHandlers.Abstractions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Discord.Extensions.InteractionHandlers;

public class SlashCommandsHandler(
    IServiceProvider provider,
    DiscordSocketClient client,
    ILogger<SlashCommandsHandler> logger,
    SlashCommandHandlerOptions options
) : ISlashCommandHandler
{
    public async ValueTask RegisterAsync()
    {
        await using var scope = provider.CreateAsyncScope();
        var commands = scope.ServiceProvider.GetServices<ISlashCommand>();

        int count = 0;

        if (!await options.RegisterCheck(provider))
        {
            logger.LogInformation("Skipping registration of slash commands");
            return;
        }

        if (options.RegisterForGuild.HasValue)
        {
            count = await RegisterGuildCommands(commands, count, options.RegisterForGuild.Value);
        }
        else
        {
            count = await RegisterGlobalCommands(commands, count);
        }

        logger.LogInformation("Registered {Count} slash commands", count);
    }

    public Task HandleAsync(SocketSlashCommand command)
    {
        // This method is called by the discord socket client
        // To avoid blocking the client, we run the command handling in a separate task
        _ = Task.Run(async () =>
        {
            try
            {
                await using var scope = provider.CreateAsyncScope();
                var commands = scope.ServiceProvider.GetServices<ISlashCommand>();
                var targetCommand = commands.FirstOrDefault(x => x.Name == command.Data.Name);
                if (targetCommand is null)
                {
                    logger.LogError("Command {CommandName} can't be handled", command.Data.Name);
                    await command.RespondAsync(
                        "Command not found. If you think this is an error, please contact the bot owner.");
                    return;
                }

                await targetCommand.HandleAsync(command);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to handle command {CommandName}", command.Data.Name);
                await command.RespondAsync("An error occurred while handling the command. Please try again later.",
                    ephemeral: true);
            }
        });
        return Task.CompletedTask;
    }


    private async Task<int> RegisterGlobalCommands(IEnumerable<ISlashCommand> commands, int count)
    {
        // todo: to parallel with limited concurrency
        var registrationTasks = commands.Select(async command =>
        {
            logger.LogDebug("Registering command {CommandName}", command.Name);
            try
            {
                var builder = new SlashCommandBuilder()
                    .WithName(command.Name)
                    .WithDescription(command.Description);
                builder = await command.RegisterAsync(builder);
                await client.CreateGlobalApplicationCommandAsync(builder.Build());
                Interlocked.Increment(ref count);
                logger.LogDebug("Done registering command {CommandName}", command.Name);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to register command {CommandName}", command.Name);
            }
        });

        await Task.WhenAll(registrationTasks);
        return count;
    }

    private async Task<int> RegisterGuildCommands(IEnumerable<ISlashCommand> commands, int count, ulong guildId)
    {
        var registrationTasks = commands.Select(async command =>
        {
            logger.LogDebug("Registering command {CommandName}", command.Name);
            try
            {
                var builder = new SlashCommandBuilder()
                    .WithName(command.Name)
                    .WithDescription(command.Description);
                builder = await command.RegisterAsync(builder);
                await client.Rest.CreateGuildCommand(builder.Build(), guildId);
                Interlocked.Increment(ref count);
                logger.LogDebug("Done registering command {CommandName}", command.Name);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to register command {CommandName}", command.Name);
            }
        });

        await Task.WhenAll(registrationTasks);
        return count;
    }
}