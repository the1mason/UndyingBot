using System.Reflection;
using Discord.Extensions.InteractionHandlers;
using Discord.Extensions.InteractionHandlers.Abstractions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Discord.Extensions;

public static class InteractionExtensions
{
    public static void AddSlashCommands(this IServiceCollection services,
        Assembly scanAssembly,
        ISlashCommandHandler? handler = null,
        SlashCommandHandlerOptions? opts = null)
    {
        opts ??= new();
        services.TryAddSingleton(opts);

        if (handler is not null)
            services.AddSingleton(handler);
        else
            services.AddSingleton<ISlashCommandHandler, SlashCommandsHandler>();

        foreach (var type in scanAssembly.GetTypes())
        {
            if (type.IsAssignableTo(typeof(ISlashCommand)))
            {
                services.AddTransient(typeof(ISlashCommand), type);
            }
        }
    }

    public static async Task UseSlashCommands(this IServiceProvider provider)
    {
        var client = provider.GetRequiredService<DiscordSocketClient>();
        var slashCommandHandler = provider.GetRequiredService<ISlashCommandHandler>();
        client.SlashCommandExecuted += slashCommandHandler.HandleAsync;
        await slashCommandHandler.RegisterAsync();
    }
}