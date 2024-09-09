using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SneikbotDiscord.Sneik;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneikbotDiscord.Commands.Prefix
{
    public class MarkovModule : BaseCommandModule
    {
        [Command("марков")]
        [Cooldown(1, 5, CooldownBucketType.Channel)]
        [RequireGuild]
        [Description("команды модуля \"Markov\" - инфо, читай, забудь, пиши, замолчи, очистись")]
        public async Task Markov(CommandContext ctx, params string[] texts)
        {
            if(texts.Length == 0)
            {
                string prefix = SneikBot.Guilds[ctx.Guild.Id].Prefix;
                await ctx.RespondAsync(
                    $"{prefix}марков инфо - показывает информацию\n" +
                    $"{prefix}марков читай - разрешает боту запоминать слова этого канала\n" +
                    $"{prefix}марков забудь - запрещает боту запоминать слова этого канала\n" +
                    $"{prefix}марков пиши - разрешает боту писать в этом канале\n" +
                    $"{prefix}марков замолчи - запрещает боту писать в этом канале\n" +
                    $"{prefix}марков очистись - стирает память для этого сервера");
                return;
            }
            else
            {
                if (texts[0].ToLower() == "инфо")
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Title = "Информация по модулю Markov",
                        Description = $"**Выучено слов:** {SneikBot.markovChain[ctx.Guild.Id].chain.Count}\n\n" +
                            $"**Прослушиваемые каналы**: {SneikBot.Guilds[ctx.Guild.Id].MarkovReadingChannels.Count}\n" +
                            $"**Каналы для генерации**: {SneikBot.Guilds[ctx.Guild.Id].MarkovWritingChannels.Count}\n" +
                            $"\n" +
                            $"Данный канал читается? : {SneikBot.Guilds[ctx.Guild.Id].MarkovReadingChannels.Contains(ctx.Channel.Id)}\n" +
                            $"Данный канал для генерации? : {SneikBot.Guilds[ctx.Guild.Id].MarkovWritingChannels.Contains(ctx.Channel.Id)}",
                        Color = DiscordColor.Orange
                    };
                    await ctx.RespondAsync(embed);
                    return;
                }

                if (ctx.Member.IsOwner)
                {
                    switch (texts[0].ToLower())
                    {
                        case "читай":
                            if (SneikBot.Guilds[ctx.Guild.Id].MarkovReadingChannels.Contains(ctx.Channel.Id))
                            {
                                var embed2 = new DiscordEmbedBuilder
                                {
                                    Title = ":no_entry: Данный канал уже прослушивается",
                                    Color = DiscordColor.HotPink
                                };
                                await ctx.RespondAsync(embed2);
                            }
                            else
                            {
                                SneikBot.Guilds[ctx.Guild.Id].MarkovReadingChannels.Add(ctx.Channel.Id);
                                var embed2 = new DiscordEmbedBuilder
                                {
                                    Title = ":white_check_mark: Канал теперь прослушивается",
                                    Color = DiscordColor.SpringGreen
                                };
                                await ctx.RespondAsync(embed2);
                            }
                            break;
                        case "забудь":
                            if (SneikBot.Guilds[ctx.Guild.Id].MarkovReadingChannels.Contains(ctx.Channel.Id))
                            {
                                SneikBot.Guilds[ctx.Guild.Id].MarkovReadingChannels.Remove(ctx.Channel.Id);
                                var embed2 = new DiscordEmbedBuilder
                                {
                                    Title = ":white_check_mark: Канал больше не прослушивается",
                                    Color = DiscordColor.SpringGreen
                                };
                                await ctx.RespondAsync(embed2);
                            }
                            else
                            {
                                var embed2 = new DiscordEmbedBuilder
                                {
                                    Title = ":no_entry: Данный канал не находится в списке прослушиваемых",
                                    Color = DiscordColor.HotPink
                                };
                                await ctx.RespondAsync(embed2);
                            }
                            break;

                        case "пиши":
                            if (SneikBot.Guilds[ctx.Guild.Id].MarkovWritingChannels.Contains(ctx.Channel.Id))
                            {
                                var embed2 = new DiscordEmbedBuilder
                                {
                                    Title = ":no_entry: Данный канал уже в списке для генерации",
                                    Color = DiscordColor.HotPink
                                };
                                await ctx.RespondAsync(embed2);
                            }
                            else
                            {
                                SneikBot.Guilds[ctx.Guild.Id].MarkovWritingChannels.Add(ctx.Channel.Id);
                                var embed2 = new DiscordEmbedBuilder
                                {
                                    Title = ":white_check_mark: Канал теперь в списке генерации",
                                    Color = DiscordColor.SpringGreen
                                };
                                await ctx.RespondAsync(embed2);
                            }
                            break;
                        case "замолчи":
                            if (SneikBot.Guilds[ctx.Guild.Id].MarkovWritingChannels.Contains(ctx.Channel.Id))
                            {
                                SneikBot.Guilds[ctx.Guild.Id].MarkovWritingChannels.Remove(ctx.Channel.Id);
                                var embed2 = new DiscordEmbedBuilder
                                {
                                    Title = ":white_check_mark: Канал больше не в списке генерации",
                                    Color = DiscordColor.SpringGreen
                                };
                                await ctx.RespondAsync(embed2);
                            }
                            else
                            {
                                var embed2 = new DiscordEmbedBuilder
                                {
                                    Title = ":no_entry: Данный канал не находится в списке генерации",
                                    Color = DiscordColor.HotPink
                                };
                                await ctx.RespondAsync(embed2);
                            }
                            break;
                        case "очистись":
                            if (SneikBot.markovChain[ctx.Channel.Id].chain.Count != 0)
                            {
                                SneikBot.markovChain[ctx.Channel.Id].chain.Clear();
                                var embed2 = new DiscordEmbedBuilder
                                {
                                    Title = ":white_check_mark: Память успешно очищена",
                                    Color = DiscordColor.SpringGreen
                                };
                                await ctx.RespondAsync(embed2);
                            }
                            else
                            {
                                var embed2 = new DiscordEmbedBuilder
                                {
                                    Title = ":no_entry: Память уже пуста",
                                    Color = DiscordColor.HotPink
                                };
                                await ctx.RespondAsync(embed2);
                            }
                            break;
                        default:
                            break;
                    }
                    return;
                }
                var finalEmbed = new DiscordEmbedBuilder
                {
                    Title = "## :white_check_mark: Неверные аргументы",
                    Color = DiscordColor.Red
                };
                await ctx.RespondAsync(finalEmbed);
            }
        }
    }
}
