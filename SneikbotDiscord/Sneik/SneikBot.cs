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
using SneikbotDiscord.DataBase;

namespace SneikbotDiscord.Sneik
{
    public class SneikBot
    {
        public static event Action<string> OnLog = delegate { };

        public static readonly JSONDataHandler jsonHandler = new JSONDataHandler();
        private static DiscordClient discord;
        private static CommandsNextExtension commands;
        private static SlashCommandsExtension slash;
        public static Dictionary<ulong, MarkovChain> markovChain = new Dictionary<ulong, MarkovChain>();

        public static Dictionary<ulong, GuildData> Guilds = new Dictionary<ulong, GuildData>();

        public static async Task Start()
        {
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
                EnableDefaultHelp = false,

                PrefixResolver = async (msg) =>
                {
                    var guildId = msg.Channel.GuildId;

                    //var customprefixdata = await jsonHandler.GetAllGuildDataFromJSON((ulong)guildId);
                    var customprefixdata = Guilds[guildId.Value];

                    if (customprefixdata.Prefix != null)
                    {
                        return msg.GetStringPrefixLength(customprefixdata.Prefix);
                    }
                    else
                    {
                        return msg.GetStringPrefixLength(botConfig.Prefix);
                    }
                }
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
            string markovPath = $"{AppDomain.CurrentDomain.BaseDirectory}\\MarkovWords";
            if (Directory.Exists(markovPath) == false)
            {
                Directory.CreateDirectory(markovPath);
            }

            foreach (var guild in markovChain)
            {
                string path = $"{AppDomain.CurrentDomain.BaseDirectory}\\MarkovWords\\{guild.Key}.json";
                var markovWords = JsonConvert.SerializeObject(guild.Value.chain);

                File.WriteAllText(path, markovWords);
            }
            foreach(var guild in Guilds)
            {
                await jsonHandler.SaveGuildDataToJSON(guild.Value, guild.Key);
            }

            await ModifyBotNickname($"Sneik (выключен {DateTime.Now.ToShortTimeString()} по МСК)",true);
            //discord.DisconnectAsync().GetAwaiter().GetResult();
        }

        private static async Task OnReady(DiscordClient sender, ReadyEventArgs e)
        {
            string markovPath = $"{AppDomain.CurrentDomain.BaseDirectory}\\MarkovWords";
            if (Directory.Exists(markovPath) == false)
            {
                Directory.CreateDirectory(markovPath);
            }

            foreach (var guild in sender.Guilds)
            {
                var guildData = await jsonHandler.GetAllGuildDataFromJSON(guild.Key);
                if (guildData == null) guildData = new GuildData(guild.Key, "!");

                Guilds.Add(guildData.ID, guildData);

                string path = $"{AppDomain.CurrentDomain.BaseDirectory}\\MarkovWords\\{guildData.ID}.json";
                if (File.Exists(path))
                {
                    var markovWords = File.ReadAllText(path);
                    var guildMarkov = new MarkovChain();
                    guildMarkov.ApplyWords(JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(markovWords));

                    markovChain.Add(guildData.ID, guildMarkov);
                }
            }

            OnLog("Bot is connected and ready!");
            await CreatePaths();

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
        public static async Task CreatePaths()
        {
            //var vc = JSONDataHandler.DATA_TYPES.USERVC;
            var guild = JSONDataHandler.DATA_TYPES.GUILD;
            //var category = JSONDataHandler.DATA_TYPES.CATEGORY;
            var channel = JSONDataHandler.DATA_TYPES.CHANNEL;
            //var role = JSONDataHandler.DATA_TYPES.ROLE;
            //var message = JSONDataHandler.DATA_TYPES.MESSAGE;
            //var vccategory = JSONDataHandler.DATA_CATEGORIES.PrivateVCUserData;
            var guildcategory = JSONDataHandler.DATA_CATEGORIES.GuildData;
            //var categorycategory = JSONDataHandler.DATA_CATEGORIES.CategoryData;
            var channelcategory = JSONDataHandler.DATA_CATEGORIES.ChannelData;
            //var rolecategory = JSONDataHandler.DATA_CATEGORIES.RoleData;
            //var messagecategory = JSONDataHandler.DATA_CATEGORIES.MessageData;

            //jsonHandler.CreatePathIfNotExists(vc, vccategory);
            jsonHandler.CreatePathIfNotExists(guild, guildcategory);
            //jsonHandler.CreatePathIfNotExists(category, categorycategory);
            jsonHandler.CreatePathIfNotExists(channel, channelcategory);
            //jsonHandler.CreatePathIfNotExists(role, rolecategory);
            //jsonHandler.CreatePathIfNotExists(message, messagecategory);

            await Task.CompletedTask;
        }



        //static List<string> collectedMessages = new List<string>();
        private static async Task OnMessageCreated(DiscordClient sender, MessageCreateEventArgs e)
        {
            if (e.Author.IsBot) return;
            OnLog($"Server {e.Guild.Name} -> {e.Channel.Name} -> {e.Author.Username}: {e.Message.Content}");
            
            // Сохраняем сообщения
            if (e.Message.Content.StartsWith(Guilds[e.Guild.Id].Prefix) == false)
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
                markovChain[e.Guild.Id].AddWords(words);

                sentence = string.Join(" ",words);
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
                    string response = markovChain[e.Guild.Id].GenerateSentence(markovChain[e.Guild.Id].GetRandomStartWord(), new Random().Next(3,25));

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
