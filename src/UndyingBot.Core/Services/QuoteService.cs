using Dapper;
using Microsoft.Extensions.Options;
using UndyingBot.Core.Db;
using UndyingBot.Core.Models;
using UndyingBot.Core.Options;

namespace UndyingBot.Core.Services;

public sealed class QuoteService(IOptionsMonitor<QuoteOptions> quoteOptions, SqliteConnectionFactory connectionFactory)
{
    public async Task<Quote?> GetRandomQuoteAsync()
    {
        var quotes = await GetAllQuotesAsync();
        if(quotes.Length == 0)
        {
            return null;
        }
        var index = Random.Shared.Next(0, quotes.Length);
        return quotes[index];
    }

    public async Task<Quote?> GetQuoteOfTheDayAsync(ulong id)
    {
        var quotes = await GetAllQuotesAsync();
        if(quotes.Length == 0)
        {
            return null;
        }
        var index = (id + (ulong)DateTime.UtcNow.DayOfYear) % (ulong)quotes.Length;
        return quotes[index];
    }
    
    public async Task<bool> AddQuoteAsync(Quote quote, ulong userId)
    {
        if(!quoteOptions.CurrentValue.Editors.Contains(userId.ToString()))
        {
            return false;
        }
        const string sql = "insert into Quotes (Text, Author, Url) values (@Text, @Author, @Url);";
        await using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(sql, quote) > 0;
    }
    
    public async Task<bool> RemoveQuoteAsync(string? text, string? url, ulong userId)
    {
        if(!quoteOptions.CurrentValue.Editors.Contains(userId.ToString()))
            return false;

        const string sql = "delete from Quotes where Text = @text or Url = @url;";
        await using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(sql, new { text, url }) > 0;
    }
    
    private async Task<Quote[]> GetAllQuotesAsync()
    {
        const string sql = "select * from Quotes;";
        await using var connection = connectionFactory.CreateConnection();
        return (await connection.QueryAsync<Quote>(sql)).ToArray();
    }
}