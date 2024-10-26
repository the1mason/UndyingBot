using Discord.WebSocket;

namespace Discord.Extensions.InteractionHandlers.Abstractions;

public interface ISlashCommand
{
    public string Name { get; }
    public string Description { get; }
    ValueTask<SlashCommandBuilder> RegisterAsync(SlashCommandBuilder builder);
    Task HandleAsync(SocketSlashCommand command);
}