namespace Discord.Extensions.InteractionHandlers;

public sealed class SlashCommandHandlerOptions
{
    public Func<IServiceProvider,ValueTask<bool>> RegisterCheck { get; set; } = _ => ValueTask.FromResult(true);
    public ulong? RegisterForGuild { get; set; }
}