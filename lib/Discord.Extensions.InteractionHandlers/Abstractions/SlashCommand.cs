using Discord.WebSocket;

namespace Discord.Extensions.InteractionHandlers.Abstractions;

public abstract class SlashCommand : ISlashCommand
{
    public abstract string Name { get; }
    public abstract string Description { get; }
    public virtual ValueTask<SlashCommandBuilder> RegisterAsync(SlashCommandBuilder builder)
    {
        return ValueTask.FromResult(builder);
    }
    public abstract Task HandleAsync(SocketSlashCommand command);
}