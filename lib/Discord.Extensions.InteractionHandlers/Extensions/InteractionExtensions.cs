using System.Reflection;
using Discord.Extensions.InteractionHandlers;
using Discord.Extensions.InteractionHandlers.Abstractions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Discord.Extensions;

public static class InteractionExtensions
{
    public static IServiceCollection AddSlashCommands(
        this IServiceCollection services,
        Assembly? scanAssembly,
        ISlashCommandHandler? handler = null)
    {
        
        if(handler is not null)
            services.AddSingleton(handler);
        else
            services.AddSingleton<ISlashCommandHandler, SlashCommandsHandler>();
        
        foreach (var type in scanAssembly.GetTypes())
        {
            if (type.IsAssignableTo(typeof(ISlashCommand)))
            {
                services.AddSingleton(typeof(ISlashCommand) ,type);
            }
        }

        return services;
    }

    public static async Task UseSlashCommands(this IServiceProvider provider)
    {
        var client = provider.GetRequiredService<DiscordSocketClient>();
        var slashCommandHandler = provider.GetRequiredService<ISlashCommandHandler>();
        client.SlashCommandExecuted += slashCommandHandler.HandleAsync;
        await slashCommandHandler.RegisterAsync();
    }
}