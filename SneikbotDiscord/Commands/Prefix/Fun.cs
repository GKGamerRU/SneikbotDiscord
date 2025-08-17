using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SneikbotDiscord.Sneik;
using SneikbotDiscord.Utils;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SneikbotDiscord.Commands.Prefix
{
    public sealed class Fun : BaseCommandModule
    {
        //static List<BaseProvider> providers;
        //static AutoProvider autoProvider = new AutoProvider();

        static LmStudioProvider lmStudioProvider = new LmStudioProvider();
        static OllamaProvider ollamaProvider = new OllamaProvider();

        public static void ApplyLocalProvider()
        {
            if (SneikBot.botConfig.LocalNeuralProvider == "ollama")
            {
                ollamaProvider.SetSystemPrompt(SneikBot.botConfig.SystemPrompt);
                ollamaProvider._baseUrl = SneikBot.botConfig.NeuralAdresse;
            }
            else if (SneikBot.botConfig.LocalNeuralProvider == "lm studio")
            {
                lmStudioProvider.SetSystemPrompt(SneikBot.botConfig.SystemPrompt);
                lmStudioProvider._baseUrl = SneikBot.botConfig.NeuralAdresse;
            }
        }
        public static void ApplyAuto()
        {
            //providers = new List<BaseProvider>()
            //{ 
            //    new AIProvider.Providers.gpt4free(),
            //    new AIProvider.Providers.Kobolt(),
            //    new AIProvider.Providers.Ollama(),
            //    new BlackBox(),
            //    new PizzaGPT(),
            //    new Zephyr()
            //};
            //providers[0].SystemPrompt = "assistent представляется как Снейк - добрый, умный, хитрый и игривый удав. Отвечает кратко собеседнику на его языке.";
            //providers[1].SystemPrompt = "assistent представляется как Снейк - добрый, умный, хитрый и игривый удав. Отвечает кратко собеседнику на его языке.";
            //providers[2].SystemPrompt = "assistent представляется как Снейк - добрый, умный, хитрый и игривый удав. Отвечает кратко собеседнику на его языке.";
            //providers[3].SystemPrompt = "assistent представляется как Снейк - добрый, умный, хитрый и игривый удав. Отвечает кратко собеседнику на его языке.";
            //providers[4].SystemPrompt = "assistent представляется как Снейк - добрый, умный, хитрый и игривый удав. Отвечает кратко собеседнику на его языке.";
            //providers[5].SystemPrompt = "assistent представляется как Снейк - добрый, умный, хитрый и игривый удав. Отвечает кратко собеседнику на его языке.";

            //autoProvider.AddService(providers[2],"None","Gemma3:12b");
            //autoProvider.AddService(providers[3], "None", null);
            //autoProvider.AddService(providers[0], "Liaobots", "claude-3-sonnet-20240229");
            //autoProvider.AddService(providers[0], "Blackbox", null);
            //autoProvider.AddService(providers[0], "HuggingFace", "mistralai/Mixtral-8x7B-Instruct-v0.1");
            //autoProvider.AddService(providers[0], "HuggingFace", "mistralai/Mistral-7B-Instruct-v0.2");
            //autoProvider.AddService(providers[0], "Pi", null);
            //autoProvider.AddService(providers[4], "None", null);
            //autoProvider.AddService(providers[5], "None", null);
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
            if (SneikBot.Guilds[ctx.Guild.Id].MarkovWritingChannels.Contains(ctx.Channel.Id) == false || isProcessing) return;

            if (texts.Length > 478) { await ctx.RespondAsync("***:warning: Лимит слов превышен! (максимально 478 слов)***"); return; }
            if (texts.Length == 0) { await ctx.RespondAsync("***:warning: Вы не передали аргументы!***"); return; }

            var message = string.Join(" ", texts);
            DiscordMessage dsMessage = ctx.Message; 
            dsMessage = await ctx.RespondAsync(":hourglass_flowing_sand:`подождите...`");
            Prompt prompt = new Prompt();
            prompt.Text = message;
            prompt.ModelName = SneikBot.botConfig.ModelProvider;

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

            Response result = new Response() { message = "AI is inactive now.", isError = true };
            if(SneikBot.botConfig.LocalNeuralProvider == "ollama")
            {
                result = await ollamaProvider.SendMessageAsync(prompt, false);
            } 
            else if (SneikBot.botConfig.LocalNeuralProvider == "lm studio")
            {
                result = await lmStudioProvider.SendMessageAsync(prompt, false);
            }

            await dsMessage.ModifyAsync(new DiscordMessageBuilder().WithContent($"{ctx.Member.Mention} {result.message}"));
            isProcessing = false;
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

        [Command("dm")]
        [Cooldown(1, 5, CooldownBucketType.Channel)]
        [RequireGuild]
        [Description("Try create Private Channel")]
        public async Task dm(CommandContext ctx)
        {
            var dmChannel = await ctx.Member.CreateDmChannelAsync();
            await dmChannel.SendMessageAsync("Привет! Это личное сообщение от бота.");
        }

        [Command("crashword")]
        [Cooldown(1,5,CooldownBucketType.Channel)]
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
