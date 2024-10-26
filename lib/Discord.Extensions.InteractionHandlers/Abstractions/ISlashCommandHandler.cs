using Discord.WebSocket;

namespace Discord.Extensions.InteractionHandlers.Abstractions;

public interface ISlashCommandHandler
{
    ValueTask RegisterAsync();
    Task HandleAsync(SocketSlashCommand command);
}