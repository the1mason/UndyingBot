using Discord;
using Discord.Extensions.InteractionHandlers.Abstractions;
using Discord.WebSocket;
using UndyingBot.Core.Models;
using UndyingBot.Core.Services;

namespace UndyingBot.Discord.SlashCommands;

public class AddQuoteCommand(QuoteService service) : SlashCommand
{
    public override string Name => "add-quote";
    public override string Description => "Добавить цитату (для администраторов)";

    public override ValueTask<SlashCommandBuilder> RegisterAsync(SlashCommandBuilder builder)
    {
        builder.AddOption("url", ApplicationCommandOptionType.String, "Ссылка на цитату", isRequired:false);
        builder.AddOption("text", ApplicationCommandOptionType.String, "Текст цитаты", isRequired:false);
        builder.AddOption("author", ApplicationCommandOptionType.String, "Автор цитаты", isRequired:false);
        return ValueTask.FromResult(builder);
    }
    public override async Task HandleAsync(SocketSlashCommand command)
    {
        var url = command.Data.Options.FirstOrDefault(x => x.Name == "url")?.Value as string;
        var text = command.Data.Options.FirstOrDefault(x => x.Name == "text")?.Value as string;
        var author = command.Data.Options.FirstOrDefault(x => x.Name == "author")?.Value as string;
        
        if(url is null && text is null)
        {
            await command.RespondAsync("Необходимо указать хотя бы один параметр: url или text", ephemeral:true);
            return;
        }

        var quote = new Quote
        {
            Text = text,
            Url = url,
            Author = author
        };
        var result = await service.AddQuoteAsync(quote, command.User.Id);
        if(result)
        {
            await command.RespondAsync("Цитата успешно добавлена");
        }
        else
        {
            await command.RespondAsync("Недостаточно прав", ephemeral:true);
        }
    }
}