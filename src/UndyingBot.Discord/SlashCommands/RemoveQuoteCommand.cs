using Discord;
using Discord.Extensions.InteractionHandlers.Abstractions;
using Discord.WebSocket;
using UndyingBot.Core.Models;
using UndyingBot.Core.Services;

namespace UndyingBot.Discord.SlashCommands;

public class RemoveQuoteCommand(QuoteService service) : SlashCommand
{
    public override string Name => "remvoe-quote";
    public override string Description => "Удалить цитату (для администраторов)";

    public override ValueTask<SlashCommandBuilder> RegisterAsync(SlashCommandBuilder builder)
    {
        builder.AddOption("url", ApplicationCommandOptionType.String, "Ссылка на цитату", isRequired:false);
        builder.AddOption("text", ApplicationCommandOptionType.String, "Текст цитаты", isRequired:false);
        return ValueTask.FromResult(builder);
    }
    public override async Task HandleAsync(SocketSlashCommand command)
    {
        var url = command.Data.Options.FirstOrDefault(x => x.Name == "url")?.Value as string;
        var text = command.Data.Options.FirstOrDefault(x => x.Name == "text")?.Value as string;
        
        if(url is null && text is null)
        {
            await command.RespondAsync("Необходимо указать хотя бы один параметр: url или text", ephemeral:true);
            return;
        }

        var result = await service.RemoveQuoteAsync(text, url, command.User.Id);
        if(result)
        {
            await command.RespondAsync("Цитата успешно удалена");
        }
        else
        {
            await command.RespondAsync("Недостаточно прав или цитата не найдена", ephemeral:true);
        }
    }
}