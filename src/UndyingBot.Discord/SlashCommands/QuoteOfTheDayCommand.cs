using Discord;
using Discord.Extensions.InteractionHandlers.Abstractions;
using Discord.WebSocket;
using UndyingBot.Core.Services;

namespace UndyingBot.Discord.SlashCommands;

public sealed class QuoteOfTheDayCommand(QuoteService service) : SlashCommand
{
    public override string Name => "quote-of-the-day";
    public override string Description => "Получить цитату дня";
    public override async Task HandleAsync(SocketSlashCommand command)
    {
        var quote = await service.GetQuoteOfTheDayAsync(command.User.Id);
        if(quote is null)
        {
            await command.RespondAsync("Цитаты не найдены");
            return;
        }

        var embed = new EmbedBuilder().WithColor(Color.Red).WithAuthor("Цитата дня для " + command.User.Username);
        
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