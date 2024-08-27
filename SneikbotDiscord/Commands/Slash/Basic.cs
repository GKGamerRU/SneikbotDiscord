using DSharpPlus.SlashCommands;
using System.Threading.Tasks;

namespace SneikbotDiscord.Commands.Slash
{
    [SlashCommandGroup("Basic", "Basic Commands")]
    public sealed class Basic : ApplicationCommandModule
    {
        [SlashCommand("ping", "Replices with pong!")]
        public async Task Ping(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DSharpPlus.Entities.DiscordInteractionResponseBuilder().WithContent("Pong!"));
        }
    }
}
