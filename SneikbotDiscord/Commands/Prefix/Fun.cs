using AIProvider;
using AIProvider.Providers;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SneikbotDiscord.Sneik;
using SneikbotDiscord.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SneikbotDiscord.Commands.Prefix
{
    public sealed class Fun : BaseCommandModule
    {
        static List<BaseProvider> providers;
        static AutoProvider autoProvider = new AutoProvider();
        public static void ApplyAuto()
        {
            providers = new List<BaseProvider>()
            { 
                new AIProvider.Providers.gpt4free(),
                new AIProvider.Providers.Kobolt(),
                new AIProvider.Providers.Ollama(),
                new BlackBox(),
                new PizzaGPT(),
                new Zephyr()
            };
            providers[0].SystemPrompt = "assistent представляется как Снейк - добрый, умный, хитрый и игривый удав. Отвечает кратко собеседнику на его языке.";
            providers[1].SystemPrompt = "assistent представляется как Снейк - добрый, умный, хитрый и игривый удав. Отвечает кратко собеседнику на его языке.";
            providers[2].SystemPrompt = "assistent представляется как Снейк - добрый, умный, хитрый и игривый удав. Отвечает кратко собеседнику на его языке.";
            providers[3].SystemPrompt = "assistent представляется как Снейк - добрый, умный, хитрый и игривый удав. Отвечает кратко собеседнику на его языке.";
            providers[4].SystemPrompt = "assistent представляется как Снейк - добрый, умный, хитрый и игривый удав. Отвечает кратко собеседнику на его языке.";
            providers[5].SystemPrompt = "assistent представляется как Снейк - добрый, умный, хитрый и игривый удав. Отвечает кратко собеседнику на его языке.";

            autoProvider.AddService(providers[2],"None","Llava-phi3:latest");
            autoProvider.AddService(providers[3], "None", null);
            autoProvider.AddService(providers[0], "Liaobots", "claude-3-sonnet-20240229");
            autoProvider.AddService(providers[0], "Blackbox", null);
            autoProvider.AddService(providers[0], "HuggingFace", "mistralai/Mixtral-8x7B-Instruct-v0.1");
            autoProvider.AddService(providers[0], "HuggingFace", "mistralai/Mistral-7B-Instruct-v0.2");
            autoProvider.AddService(providers[0], "Pi", null);
            autoProvider.AddService(providers[4], "None", null);
            autoProvider.AddService(providers[5], "None", null);
        }

        [Command("ping")]
        [Cooldown(1, 5, CooldownBucketType.Channel)]
        [RequireGuild]
        [Description("Проверяет время отклика")]
        public async Task Ping(CommandContext ctx, string type = null)
        {
            if(type == null)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Ошибка",
                    Description = "Вы не передали тип запроса\n**Аргументы: Embed, Text.**",
                    Color = DiscordColor.HotPink
                };
                await ctx.RespondAsync(embed: embed);
            }
            else
            {
                type = type.ToLower();
                if (type == "embed")
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Title = "Отлично!",
                        Description = $"{ctx.User.Username}: {ctx.Client.Ping}ms",
                        Color = DiscordColor.Green
                    };
                    await ctx.RespondAsync(embed: embed);
                }
                else if (type == "text")
                {
                    await ctx.RespondAsync($"{ctx.User.Username}: {ctx.Client.Ping}ms");
                }
            }
        }

        [Command("coin")]
        [Cooldown(1, 5, CooldownBucketType.Channel)]
        [RequireGuild]
        [Description("Подбрасывает монетку")]
        public async Task CoinFlip(CommandContext ctx)
        {
            var random = new Random();
            var result = random.Next(0, 2);
            if (result == 0)
            {
                await ctx.RespondAsync("Орел!");
            }
            else
            {
                await ctx.RespondAsync("Решка!");
            }
        }

        bool isProcessing = false;
        [Command("ии")]
        [Cooldown(1, 5, CooldownBucketType.Channel)]
        [RequireGuild]
        [Description("Генерирует ответ с помощью ИИ(1-5минуты)")]
        public async Task AI(CommandContext ctx, params string[] texts)
        {
            if (ctx.Channel.Name != "gentai" || isProcessing) return;
            if (texts.Length > 478) { await ctx.RespondAsync("***:warning: Лимит слов превышен! (максимально 478 слов)***"); isProcessing = false; return; }
            if (texts.Length == 0) { await ctx.RespondAsync("***:warning: Вы не передали аргументы!***"); isProcessing = false; return; }

            var message = string.Join(" ", texts);
            DiscordMessage dsMessage = ctx.Message; 
            dsMessage = await ctx.RespondAsync(":hourglass_flowing_sand:`подождите...`");
            Prompt prompt = new Prompt();
            prompt.Text = message;

            if (ctx.Message.Attachments != null && ctx.Message.Attachments.Count > 0)
            {
                dsMessage = await dsMessage.ModifyAsync(":park:`Загружаю картинку...`");
                if (ImageUtils.IsImage(ctx.Message.Attachments[0].FileName) == false) return;
                var bitmap = await ImageUtils.GetImageFromUrlAsync(ctx.Message.Attachments[0].Url);
                using (MemoryStream ms = new MemoryStream())
                {
                    bitmap.Save(ms, ImageFormat.Png);
                    prompt.SetBase64Image(Convert.ToBase64String(ms.ToArray()));
                }
                bitmap.Dispose();
            }

            dsMessage = await dsMessage.ModifyAsync(":hourglass_flowing_sand:`Генерирую ответ... Это займет 1-5 минут...`");
            var result = await autoProvider.SendMessage(prompt, false);
            
            await dsMessage.ModifyAsync(new DiscordMessageBuilder().WithContent($"{ctx.Member.Mention} {result.message}"));
            isProcessing = false;
        }

        [Command("дем")]
        [Cooldown(1, 5, CooldownBucketType.Channel)]
        [RequireGuild]
        [Description("Показывает изображение-демотиватор с бредовым текстом")]
        public async Task Demotivator(CommandContext ctx, string url = null)
        {
            if (SneikBot.Guilds[ctx.Guild.Id].MarkovWritingChannels.Contains(ctx.Channel.Id) == false) return;

            Bitmap bitmap = null;
            if (ctx.Message.Attachments.Count != 0 && ImageUtils.IsImage(ctx.Message.Attachments[0].FileName)) {
                bitmap = await ImageUtils.GetImageFromUrlAsync(ctx.Message.Attachments[0].Url);
                bitmap = Markov.MarkovImage.GenerateDemotivator(ctx.Guild, SneikBot.markovChain[ctx.Guild.Id], bitmap); 
            }else if(url != null)
            {
                bitmap = await ImageUtils.GetImageFromUrlAsync(url);
                bitmap = Markov.MarkovImage.GenerateDemotivator(ctx.Guild, SneikBot.markovChain[ctx.Guild.Id], bitmap);
            }else
                bitmap = Markov.MarkovImage.GenerateDemotivator(ctx.Guild, SneikBot.markovChain[ctx.Guild.Id]);


            var stream = new System.IO.MemoryStream();
            bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            stream.Position = 0;

            var builder = new DiscordMessageBuilder()
                        .AddFile("test.png",stream);

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
            if(texts.Length != 0) text = string.Join(" ", texts);
            StringBuilder stringBuilder = new StringBuilder();
            Random random = new Random();
            var messages = random.Next(4, 8);

            string currentMessage = text;
            if (text != null) {
                stringBuilder.AppendLine($"{ctx.User.Username}: {currentMessage}");
            }
            else
            {
                currentMessage = SneikBot.markovChain[ctx.Guild.Id].GenerateSentence(SneikBot.markovChain[ctx.Guild.Id].GetRandomStartWord(), random.Next(3, 25));
                stringBuilder.AppendLine($"{ctx.User.Username}: {currentMessage}");
            }

            var currentUser = ctx.User;
            for(int i = 0; i < messages; i++)
            {
                currentUser = currentUser == ctx.User ? ctx.Client.CurrentUser : ctx.User;

                var words = currentMessage.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var sentence = SneikBot.markovChain[ctx.Guild.Id].GenerateSentence(words[random.Next(words.Length - 1)], random.Next(3, 25));
                if(sentence == currentMessage)
                    sentence = SneikBot.markovChain[ctx.Guild.Id].GenerateSentence(SneikBot.markovChain[ctx.Guild.Id].GetRandomStartWord(), random.Next(3, 25));

                stringBuilder.AppendLine($"{currentUser.Username}: {sentence}");
                currentMessage = sentence;
            }

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

        [Command("commands")]
        [Cooldown(1, 5, CooldownBucketType.Channel)]
        [RequireGuild]
        [Description("Показывает все команды Бота")]
        public async Task Commands(CommandContext ctx)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach(var pair in ctx.CommandsNext.RegisteredCommands)
            {
                stringBuilder.AppendLine($"{pair.Value} - {pair.Value.Description}"); //{(pair.Value.CustomAttributes.Count == 0 ? null : $"Аргументы {pair.Value.CustomAttributes.Count}")}
            }

            DiscordEmbedBuilder messageBuilder = new DiscordEmbedBuilder
            {
                Title = $"{ctx.Prefix} Команды",
                Description = stringBuilder.ToString(),
                Color = DiscordColor.Red,
            };
            await ctx.RespondAsync(embed: messageBuilder);
        }

        [Command("crashword")]
        [Cooldown(1,5,CooldownBucketType.Channel)]
        [RequireGuild]
        [Description("УДаляет несколько символов в нескольких местах")]
        public async Task CrashWord(CommandContext ctx, params string[] texts)
        {
            var text = string.Join(" ", texts);

            if (text == null) 
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Ошибка",
                    Description = "Вы не передали текст.",
                    Color = DiscordColor.HotPink
                };
                await ctx.RespondAsync(embed: embed);
            }
            else
            {
                var word = new StringBuilder();

                var random = new Random();
                var result = random.Next(1, text.Length / 4);

                List<int> places = new List<int>();

                for(int i = result; i > 0; i--)
                {
                    var result2 = random.Next(0, text.Length);
                    places.Add(result2);
                }

                for(int i = 0; i < text.Length; i++)
                {
                    if (places.Contains(i))
                    {
                        continue;
                    }
                    else
                    {
                        word.Append(text[i]);
                    }
                }
                await ctx.RespondAsync(word.ToString());
            }
        }
    }
}
