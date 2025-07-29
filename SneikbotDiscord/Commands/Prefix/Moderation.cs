using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SneikbotDiscord.Sneik;
using System.Threading.Tasks;

namespace SneikbotDiscord.Commands.Prefix
{
    public class Moderation : BaseCommandModule
    {
        [Command("префикс")]
        [Cooldown(1, 5, CooldownBucketType.Channel)]
        [RequireGuild]
        [Description("просматривание или изменение префикса для этого сервера")]
        public async Task Ping(CommandContext ctx, string newPrefix = null)
        {
            if (newPrefix == null)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"Текущий префикс: {SneikBot.Guilds[ctx.Guild.Id].Prefix}",
                    Color = DiscordColor.Yellow
                };
                await ctx.RespondAsync(embed: embed);
            }
            else if(ctx.Member.IsOwner)
            {
                if (newPrefix == SneikBot.Guilds[ctx.Guild.Id].Prefix)
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Title = $":no_entry: Данный префикс уже установлен",
                        Color = DiscordColor.HotPink
                    };
                    await ctx.RespondAsync(embed: embed);
                }
                else if (newPrefix.Length > 8)
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Title = $":no_entry: Длина не должна превышать 8 символов",
                        Color = DiscordColor.HotPink
                    };
                    await ctx.RespondAsync(embed: embed);
                }
                else
                {
                    var embed2 = new DiscordEmbedBuilder
                    {
                        Title = ":white_check_mark: Префикс успешно изменен",
                        Description = $"\nПредыдущий префикс: {SneikBot.Guilds[ctx.Guild.Id].Prefix}\n" +
                            $"**Новый префикс**: {newPrefix}",
                        Color = DiscordColor.SpringGreen
                    };
                    SneikBot.Guilds[ctx.Guild.Id].Prefix = newPrefix;
                    await ctx.RespondAsync(embed2);
                }
            }
        }
    }
}
