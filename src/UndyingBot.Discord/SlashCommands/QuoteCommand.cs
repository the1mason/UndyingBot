using Discord;
using Discord.Extensions.InteractionHandlers;
using Discord.WebSocket;
using UndyingBot.Core.Services;

namespace UndyingBot.Discord.SlashCommands;

public sealed class QuoteCommand(QuoteService service) : SlashCommand
{
    public override string Name => "quote";
    public override string Description => "Получить случайную цитату";
    public override async Task HandleAsync(SocketSlashCommand command)
    {
        var quote = await service.GetRandomQuoteAsync();
        if(quote is null)
        {
            await command.RespondAsync("Цитаты не найдены");
            return;
        }

        var embed = new EmbedBuilder().WithColor(Color.Red).WithAuthor("Случайная цитата:");
        
        if (!string.IsNullOrWhiteSpace(quote.Text))
        {
            embed = embed.WithTitle(quote.Text);
        }

        if (!string.IsNullOrWhiteSpace(quote.Author))
        {
            embed = embed.WithDescription(quote.Author);
        }

        if (!string.IsNullOrWhiteSpace(quote.Url))
        {
            embed = embed.WithImageUrl(quote.Url);
        }
        
        await command.RespondAsync(embed: embed.Build());
    }
}