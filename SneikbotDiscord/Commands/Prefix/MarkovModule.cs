using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SneikbotDiscord.Sneik;
using SneikbotDiscord.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
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
        public async Task MarkovBasic(CommandContext ctx, params string[] texts)
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
        [Command("дем")]
        [Cooldown(1, 5, CooldownBucketType.Channel)]
        [RequireGuild]
        [Description("Показывает изображение-демотиватор с бредовым текстом")]
        public async Task Demotivator(CommandContext ctx, string url = null)
        {
            if (SneikBot.Guilds[ctx.Guild.Id].MarkovWritingChannels.Contains(ctx.Channel.Id) == false) return;

            Bitmap bitmap = null;
            if (ctx.Message.Attachments.Count != 0 && ImageUtils.IsImage(ctx.Message.Attachments[0].FileName))
            {
                bitmap = await ImageUtils.GetImageFromUrlAsync(ctx.Message.Attachments[0].Url);
                bitmap = Markov.MarkovImage.GenerateDemotivator(ctx.Guild, SneikBot.markovChain[ctx.Guild.Id], bitmap);
            }
            else if (url != null)
            {
                bitmap = await ImageUtils.GetImageFromUrlAsync(url);
                bitmap = Markov.MarkovImage.GenerateDemotivator(ctx.Guild, SneikBot.markovChain[ctx.Guild.Id], bitmap);
            }
            else
                bitmap = Markov.MarkovImage.GenerateDemotivator(ctx.Guild, SneikBot.markovChain[ctx.Guild.Id]);


            var stream = new System.IO.MemoryStream();
            bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            stream.Position = 0;

            var builder = new DiscordMessageBuilder()
                        .AddFile("test.png", stream);

            await ctx.RespondAsync(builder);
        }

        [Command("брух")]
        [Cooldown(1, 5, CooldownBucketType.Channel)]
        [RequireGuild]
        [Description("Показывает изображение с бредовым текстом")]
        public async Task Bruh(CommandContext ctx, string url = null)
        {
            if (SneikBot.Guilds[ctx.Guild.Id].MarkovWritingChannels.Contains(ctx.Channel.Id) == false) return;

            Bitmap bitmap = null;
            if (ctx.Message.Attachments.Count != 0 && ImageUtils.IsImage(ctx.Message.Attachments[0].FileName))
            {
                bitmap = await ImageUtils.GetImageFromUrlAsync(ctx.Message.Attachments[0].Url);
                bitmap = Markov.MarkovImage.GenerateBruh(ctx.Guild, SneikBot.markovChain[ctx.Guild.Id], bitmap);
            }
            else if (url != null)
            {
                bitmap = await ImageUtils.GetImageFromUrlAsync(url);
                bitmap = Markov.MarkovImage.GenerateBruh(ctx.Guild, SneikBot.markovChain[ctx.Guild.Id], bitmap);
            }
            else
                bitmap = Markov.MarkovImage.GenerateBruh(ctx.Guild, SneikBot.markovChain[ctx.Guild.Id]);


            var stream = new System.IO.MemoryStream();
            bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            stream.Position = 0;

            var builder = new DiscordMessageBuilder()
                        .AddFile("test.png", stream);

            await ctx.RespondAsync(builder);
        }

        [Command("жак")]
        [Cooldown(1, 5, CooldownBucketType.Channel)]
        [RequireGuild]
        [Description("Генерирует рандомную цитату Жака Фреско")]
        public async Task Jacque(CommandContext ctx, string url = null)
        {
            if (SneikBot.Guilds[ctx.Guild.Id].MarkovWritingChannels.Contains(ctx.Channel.Id) == false) return;

            Bitmap bitmap = null;
            bitmap = Markov.MarkovImage.GenerateJacque(ctx.Guild, SneikBot.markovChain[ctx.Guild.Id]);

            var stream = new System.IO.MemoryStream();
            bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
            stream.Position = 0;

            var builder = new DiscordMessageBuilder()
                        .AddFile("test.jpg", stream);

            await ctx.RespondAsync(builder);
        }

        [Command("мем")]
        [Cooldown(1, 5, CooldownBucketType.Channel)]
        [RequireGuild]
        [Description("Генерирует мем или мемную историю")]
        public async Task Meme(CommandContext ctx, string url = null)
        {
            if (SneikBot.Guilds[ctx.Guild.Id].MarkovWritingChannels.Contains(ctx.Channel.Id) == false) return;

            Bitmap bitmap = null;
            bitmap = Markov.MarkovImage.GenerateComics(ctx.Guild, SneikBot.markovChain[ctx.Guild.Id]);

            var stream = new System.IO.MemoryStream();
            bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
            stream.Position = 0;

            var builder = new DiscordMessageBuilder()
                        .AddFile("test.jpg", stream);

            await ctx.RespondAsync(builder);
        }

        [Command("рецепт")]
        [Cooldown(1, 5, CooldownBucketType.Channel)]
        [RequireGuild]
        [Description("Генерируется рандомный рецепт")]
        public async Task Recipe(CommandContext ctx, params string[] texts)
        {
            if (SneikBot.Guilds[ctx.Guild.Id].MarkovWritingChannels.Contains(ctx.Channel.Id) == false) return;

            string text = null;
            if (texts.Length != 0)
                text = string.Join(" ", texts);

            var random = new Random();
            string recipeTitle = text != null ? text : SneikBot.markovChain[ctx.Guild.Id].GenerateSentence(SneikBot.markovChain[ctx.Guild.Id].GetRandomStartWord(), random.Next(1, 3));

            StringBuilder ingredients = new StringBuilder();
            ingredients.AppendLine();
            ingredients.AppendLine("***===- Для приготовления нам потребуется -===***");

            for (int i = 0; i < random.Next(3, 10); i++)
            {
                ingredients.AppendLine($"{i + 1}. {random.Next(1, 20) * 50}гр {SneikBot.markovChain[ctx.Guild.Id].GenerateSentence(SneikBot.markovChain[ctx.Guild.Id].GetRandomStartWord(), random.Next(1, 3))}");
            }

            ingredients.AppendLine("***===- Для приготовления нам потребуется -===***");
            ingredients.AppendLine();

            string text2 = SneikBot.markovChain[ctx.Guild.Id].GenerateSentence(SneikBot.markovChain[ctx.Guild.Id].GetRandomStartWord(), random.Next(3, 6));

            ingredients.AppendLine($"**Блюдо подано! {text2}, {ctx.User.Username}!**");

            var builder = new DiscordMessageBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                {
                    Title = $"Рецепт: {recipeTitle}",
                    Description = ingredients.ToString(),
                    Color = new DiscordColor(random.Next(0, 0xFFFFFF))
                });

            await ctx.RespondAsync(builder);
        }

        [Command("диалог")]
        [Cooldown(1, 5, CooldownBucketType.Channel)]
        [RequireGuild]
        [Description("создает диалог между вами и ботом")]
        public async Task Dialog(CommandContext ctx, params string[] texts)
        {
            if (SneikBot.Guilds[ctx.Guild.Id].MarkovWritingChannels.Contains(ctx.Channel.Id) == false) return;

            string text = null;
            if (texts.Length != 0) text = string.Join(" ", texts);
            StringBuilder stringBuilder = new StringBuilder();
            Random random = new Random();
            var messages = random.Next(4, 8);

            string currentMessage = text;
            if (text != null)
            {
                stringBuilder.AppendLine($"{ctx.User.Username}: {currentMessage}");
            }
            else
            {
                currentMessage = SneikBot.markovChain[ctx.Guild.Id].GenerateSentence(SneikBot.markovChain[ctx.Guild.Id].GetRandomStartWord(), random.Next(3, 25));
                stringBuilder.AppendLine($"{ctx.User.Username}: {currentMessage}");
            }

            var currentUser = ctx.User;
            for (int i = 0; i < messages; i++)
            {
                currentUser = currentUser == ctx.User ? ctx.Client.CurrentUser : ctx.User;

                var words = currentMessage.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var sentence = SneikBot.markovChain[ctx.Guild.Id].GenerateSentence(words[random.Next(words.Length - 1)], random.Next(3, 25));
                if (sentence == currentMessage)
                    sentence = SneikBot.markovChain[ctx.Guild.Id].GenerateSentence(SneikBot.markovChain[ctx.Guild.Id].GetRandomStartWord(), random.Next(3, 25));

                stringBuilder.AppendLine($"{currentUser.Username}: {sentence}");
                currentMessage = sentence;
            }

            var builder = new DiscordMessageBuilder().WithContent(stringBuilder.ToString());
            await ctx.RespondAsync(builder);
        }

        [Command("стих")]
        [Cooldown(1, 5, CooldownBucketType.Channel)]
        [RequireGuild]
        [Description("создает стих из рандомных предложений")]
        public async Task MakeStih(CommandContext ctx, params string[] texts)
        {
            if (SneikBot.Guilds[ctx.Guild.Id].MarkovWritingChannels.Contains(ctx.Channel.Id) == false) return;

            string text = null;
            if (texts.Length != 0) text = string.Join(" ", texts);
            StringBuilder stringBuilder = new StringBuilder();
            Random random = new Random();
            var messages = random.Next(3, 6);

            string currentMessage = text;
            currentMessage = currentMessage != null ? currentMessage : SneikBot.markovChain[ctx.Guild.Id].GenerateSentence(SneikBot.markovChain[ctx.Guild.Id].GetRandomStartWord(), random.Next(1, 5));

            stringBuilder.AppendLine($"## Стих \"{currentMessage}\"");
            stringBuilder.AppendLine();

            currentMessage = SneikBot.markovChain[ctx.Guild.Id].GenerateSentence(SneikBot.markovChain[ctx.Guild.Id].GetRandomStartWord(), random.Next(5, 12));
            stringBuilder.AppendLine(currentMessage.FormatSentence());

            for (int i = 0; i < messages; i++)
            {
                var words = currentMessage.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var sentence = SneikBot.markovChain[ctx.Guild.Id].GenerateSentence(words[random.Next(words.Length - 1)], random.Next(5, 12));
                if (sentence == currentMessage)
                    sentence = SneikBot.markovChain[ctx.Guild.Id].GenerateSentence(SneikBot.markovChain[ctx.Guild.Id].GetRandomStartWord(), random.Next(5, 12));

                stringBuilder.AppendLine(sentence.FormatSentence());
                currentMessage = sentence;
            }

            stringBuilder.AppendLine();

            var finalSentense1 = SneikBot.markovChain[ctx.Guild.Id].GenerateSentence(SneikBot.markovChain[ctx.Guild.Id].GetRandomStartWord(), random.Next(5, 12));
            var words2 = currentMessage.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var finalSentense2 = SneikBot.markovChain[ctx.Guild.Id].GenerateSentence(words2[random.Next(words2.Length - 1)], random.Next(5, 12));

            stringBuilder.AppendLine(finalSentense1.FormatSentence());
            stringBuilder.AppendLine(finalSentense2.FormatSentence());
            stringBuilder.AppendLine(finalSentense1.FormatSentence());
            stringBuilder.AppendLine(finalSentense2.FormatSentence());

            var builder = new DiscordMessageBuilder().WithContent(stringBuilder.ToString());
            await ctx.RespondAsync(builder);
        }

        [Command("спросить")]
        [Cooldown(1, 5, CooldownBucketType.Channel)]
        [RequireGuild]
        [Description("генерирует ответ продолжением вашего предложения или слова.")]
        public async Task AskContinue(CommandContext ctx, params string[] texts)
        {
            if (SneikBot.Guilds[ctx.Guild.Id].MarkovWritingChannels.Contains(ctx.Channel.Id) == false) return;

            StringBuilder stringBuilder = new StringBuilder();
            Random random = new Random();

            var finalSentense1 = SneikBot.markovChain[ctx.Guild.Id].GenerateSentence(texts.Last(), random.Next(8, 25)).TrimStart();
            stringBuilder.AppendLine(finalSentense1.FormatSentence());

            var builder = new DiscordMessageBuilder().WithContent(stringBuilder.ToString());
            await ctx.RespondAsync(builder);
        }

        [Command("рандомдиалог")]
        [Cooldown(1, 5, CooldownBucketType.Channel)]
        [RequireGuild]
        [Description("создает диалог между вами и ботом из рандомных предложений")]
        public async Task RandomDialog(CommandContext ctx)
        {
            if (SneikBot.Guilds[ctx.Guild.Id].MarkovWritingChannels.Contains(ctx.Channel.Id) == false) return;

            StringBuilder stringBuilder = new StringBuilder();
            Random random = new Random();
            var messages = random.Next(4, 8);

            string currentMessage = SneikBot.markovChain[ctx.Guild.Id].GenerateSentence(SneikBot.markovChain[ctx.Guild.Id].GetRandomStartWord(), random.Next(3, 25));
            stringBuilder.AppendLine($"{ctx.User.Username}: {currentMessage}");

            var currentUser = ctx.User;
            for (int i = 0; i < messages; i++)
            {
                currentUser = currentUser == ctx.User ? ctx.Client.CurrentUser : ctx.User;

                var sentence = SneikBot.markovChain[ctx.Guild.Id].GenerateSentence(SneikBot.markovChain[ctx.Guild.Id].GetRandomStartWord(), random.Next(3, 25));
                stringBuilder.AppendLine($"{currentUser.Username}: {sentence}");
            }

            var builder = new DiscordMessageBuilder().WithContent(stringBuilder.ToString());
            await ctx.RespondAsync(builder);
        }
    }
}
