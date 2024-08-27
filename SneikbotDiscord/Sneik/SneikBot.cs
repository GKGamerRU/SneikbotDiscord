using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using DSharpPlus.CommandsNext;
using SneikbotDiscord.BotConfig;
using DSharpPlus.SlashCommands;
using SneikbotDiscord.Markov;
using Newtonsoft.Json;
using SneikbotDiscord.Utils;

namespace SneikbotDiscord.Sneik
{
    public class SneikBot
    {
        public static event Action<string> OnLog = delegate { };

        private static DiscordClient discord;
        private static CommandsNextExtension commands;
        private static SlashCommandsExtension slash;
        public static MarkovChain markovChain = new MarkovChain();

        public static async Task Start()
        {
            if (File.Exists("words.json"))
            {
                var collectedWords = File.ReadAllText("words.json");
                collectedMessages = JsonConvert.DeserializeObject<List<string>>(collectedWords);
            }
            if (File.Exists("markovWords.json"))
            {
                var markovWords = File.ReadAllText("markovWords.json");
                markovChain.ApplyWords(JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(markovWords));
            }

            var botConfig = BotConfiguration.LoadConfig();

            discord = new DiscordClient(new DiscordConfiguration
            {
                Token = botConfig.Token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents,
                MinimumLogLevel = LogLevel.Warning,
                AutoReconnect = true,
            });

            discord.Ready += OnReady;
            discord.MessageCreated += OnMessageCreated;

            discord.ComponentInteractionCreated += async (s, e) =>
            {
                if (e.Id == "btn_ping")
                {
                    //await e.Message.ModifyAsync("Ping button clicked!");
                    await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().WithContent("Ping button clicked!").AddComponents(new DiscordComponent[]
                        {
                        new DiscordButtonComponent(ButtonStyle.Primary, "btn_ping", "Ping"),
                        new DiscordButtonComponent(ButtonStyle.Secondary, "btn_pong", "Pong")
                        }));
                }
                else if (e.Id == "btn_pong")
                {
                    //await e.Message.ModifyAsync("Pong button clicked!");
                    await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().WithContent("Pong button clicked!").AddComponents(new DiscordComponent[]
                        {
                        new DiscordButtonComponent(ButtonStyle.Primary, "btn_ping", "Ping"),
                        new DiscordButtonComponent(ButtonStyle.Secondary, "btn_pong", "Pong")
                        }));
                }
            };

            var commandsConfiguration = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { botConfig.Prefix },
                EnableDms = false,
                EnableMentionPrefix = true,
                EnableDefaultHelp = false
            };

            commands = discord.UseCommandsNext(commandsConfiguration);
            commands.RegisterCommands<Commands.Prefix.Fun>();

            slash = discord.UseSlashCommands();
            slash.RegisterCommands<Commands.Slash.Basic>();

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }
        public static async Task Stop()
        {
            var collectedWords = JsonConvert.SerializeObject(collectedMessages);
            File.WriteAllText("words.json", collectedWords);

            var markovWords = JsonConvert.SerializeObject(markovChain.chain);
            File.WriteAllText("markovWords.json", markovWords);

            await ModifyBotNickname($"Sneik (выключен {DateTime.Now.ToShortTimeString()} по МСК)",true);
            //discord.DisconnectAsync().GetAwaiter().GetResult();
        }

        private static async Task OnReady(DiscordClient sender, ReadyEventArgs e)
        {
            OnLog("Bot is connected and ready!");
            OnLog($"Сервера {discord.Guilds.Count}");
            await ModifyBotNickname("Sneik");
        }
        static DiscordMessageBuilder CreatePong(string content)
        {
            var builder = new DiscordMessageBuilder()
                        .WithContent(content)
                        .AddComponents(new DiscordComponent[]
                        {
                        new DiscordButtonComponent(ButtonStyle.Primary, "btn_ping", "Ping"),
                        new DiscordButtonComponent(ButtonStyle.Secondary, "btn_pong", "Pong")
                        });

            return builder;
        }

        static List<string> collectedMessages = new List<string>();
        private static async Task OnMessageCreated(DiscordClient sender, MessageCreateEventArgs e)
        {
            if (e.Author.IsBot) return;
            OnLog($"Server {e.Guild.Name} -> {e.Channel.Name} -> {e.Author.Username}: {e.Message.Content}");

            // Сохраняем сообщения
            if (e.Message.Content.StartsWith("!") == false)
            {
                string sentence = e.Message.Content.FormatSentence().Replace("\n", " ");

                // Добавляем слова в цепь Маркова
                var words = sentence.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                for(int i = 0; i < words.Length; i++)
                {
                    var word = words[i];
                    if (i == words.Length - 1) word = word.RemoveSentenceEnds();

                    if (word.StartsWith("Http"))
                    {
                        words[i] = word.ToLower();
                    }
                    //if(word.StartsWith(":") && word.EndsWith(":"))
                    //{
                    //    words[i] = word;
                    //}
                }
                markovChain.AddWords(words);

                sentence = string.Join(" ",words);
                if (!collectedMessages.Contains(sentence))
                    collectedMessages.Add(sentence);
            }
            
            if (e.Message.Content.ToLower().StartsWith("ping") && e.Message.MentionedUsers.Contains(discord.CurrentUser))
            {
                await e.Message.RespondAsync(CreatePong("Pong!"));
            }else if (e.Message.Content.ToLower().Contains("sneik"))
            {
                await e.Message.ModifyAsync(e.Message.Content.ToLower().Replace("sneik", "||sneik||"));
            }else if (e.Message.MentionedUsers.Contains(discord.CurrentUser) && e.Message.Content.ToLower().Contains("покажи мордочку"))
            {
                var builder = new DiscordMessageBuilder()
                        .WithContent($"{e.Author.Mention} Вот он я")
                        .AddFile(new FileStream(@"C:\\Users\\super\\OneDrive\\Изображения\cartoon_boa_S652427499_St12_G7.5.jpeg", FileMode.Open));

                await e.Message.RespondAsync(builder);
            }
            else
            {
                if (e.Message.MentionedUsers.Contains(discord.CurrentUser) && e.Channel.Name == "gentai")
                {
                    //var words2 = collectedMessages[new Random().Next(collectedMessages.Count)].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    //string startWord = words2[new Random().Next(words2.Length)];
                    //string response = markovChain.GenerateSentence(startWord, new Random().Next(3,25));
                    string response = markovChain.GenerateSentence(markovChain.GetRandomStartWord(), new Random().Next(3,25));

                    if (new Random().Next(10) == 0)
                        response = response.ToUpper();

                    await e.Message.RespondAsync(response);
                }
            }
            //if (e.Message.Content.ToLower().StartsWith("!dm"))
            //{
            //    // Отправка ЛС
            //    e.Guild.Members.TryGetValue(e.Author.Id, out var member);
            //    var dmChannel = await member.CreateDmChannelAsync();
            //    await dmChannel.SendMessageAsync("Привет! Это личное сообщение от бота.");
            //}
        }

        private static async Task ModifyBotNickname(string newNick, bool isEnd = false)
        {
            // Меняем ник бота обратно на оригинальный
            foreach (var guild in discord.Guilds)
            {
                var currentMember = await guild.Value.GetMemberAsync(discord.CurrentUser.Id);
                await currentMember.ModifyAsync(x => x.Nickname = newNick);
            }

            OnLog("Ник заменен");
            if (isEnd)
            {
                await discord.DisconnectAsync();
            }
        }
    }
}
