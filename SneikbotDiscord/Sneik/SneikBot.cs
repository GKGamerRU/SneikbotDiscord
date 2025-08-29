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
using DSharpPlus.AsyncEvents;

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

        public static event Action<DiscordClient> OnServersListUpdate = delegate { };

        public static BotConfiguration botConfig = null;
        public static async Task Start()
        {
            discord = new DiscordClient(new DiscordConfiguration
            {
                Token = botConfig.Token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents | DiscordIntents.GuildMembers | DiscordIntents.Guilds,
                MinimumLogLevel = LogLevel.Warning | LogLevel.Error,
                AutoReconnect = true,
            });

            AsyncEventHandler<DiscordClient, GuildCreateEventArgs> addNew = async (s, e) =>
            {
                if (Guilds.ContainsKey(e.Guild.Id) == false)
                {
                    Guilds.Add(e.Guild.Id, new GuildData() { ID = e.Guild.Id });
                    OnServersListUpdate(discord);
                    OnLog($"NEW Server Detected: {e.Guild.Name}");
                }
                if (markovChain.ContainsKey(e.Guild.Id) == false)
                {
                    markovChain.Add(e.Guild.Id, new MarkovChain() { GuildID = e.Guild.Id });
                }
                await Task.CompletedTask;
            };

            discord.Ready += OnReady;
            discord.MessageCreated += OnMessageCreated;
            discord.GuildAvailable += addNew;
            discord.GuildCreated += addNew;
            
            discord.ComponentInteractionCreated += async (s, e) =>
            {
                if (e.Id == "btn_ping")
                {
                    await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().WithContent("Ping button clicked!").AddComponents(new DiscordComponent[]
                        {
                        new DiscordButtonComponent(ButtonStyle.Primary, "btn_ping", "Ping"),
                        new DiscordButtonComponent(ButtonStyle.Secondary, "btn_pong", "Pong")
                        }));
                }
                else if (e.Id == "btn_pong")
                {
                    await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().WithContent("Pong button clicked!").AddComponents(new DiscordComponent[]
                        {
                        new DiscordButtonComponent(ButtonStyle.Primary, "btn_ping", "Ping"),
                        new DiscordButtonComponent(ButtonStyle.Secondary, "btn_pong", "Pong")
                        }));
                }
            };

            var commandsConfiguration = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { },
                EnableDms = true,
                EnableMentionPrefix = true,
                EnableDefaultHelp = false,

                PrefixResolver = async (msg) =>
                {
                    if(msg.Channel.IsPrivate)
                        return msg.GetStringPrefixLength("!");

                    var guildId = msg.Channel.GuildId;
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
            commands.RegisterCommands<Commands.Prefix.MarkovModule>();
            commands.RegisterCommands<Commands.Prefix.Moderation>();

            slash = discord.UseSlashCommands();
            slash.RegisterCommands<Commands.Slash.Basic>();

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }
        public static async Task Stop()
        {
            string markovPath = $"{AppDomain.CurrentDomain.BaseDirectory}\\Markov";
            if (Directory.Exists(markovPath) == false)
            {
                Directory.CreateDirectory(markovPath);
            }

            foreach (var guild in markovChain)
            {
                string markovGuildPath = $"{AppDomain.CurrentDomain.BaseDirectory}\\Markov\\{guild.Key}";
                if (Directory.Exists(markovGuildPath) == false)
                {
                    Directory.CreateDirectory(markovGuildPath);
                }

                string path = $"{AppDomain.CurrentDomain.BaseDirectory}\\Markov\\{guild.Key}\\chains.json";
                var markovWords = JsonConvert.SerializeObject(guild.Value.chain);

                File.WriteAllText(path, markovWords);
            }
            foreach(var guild in Guilds)
            {
                await jsonHandler.DeleteGuildData(guild.Key);
                await jsonHandler.SaveGuildDataToJSON(guild.Value, guild.Key);
            }

            //await ModifyBotNickname($"Sneik (выключен {DateTime.Now.ToShortTimeString()} по МСК)",true);
        }

        private static async Task OnReady(DiscordClient sender, ReadyEventArgs e)
        {
            string markovPath = $"{AppDomain.CurrentDomain.BaseDirectory}\\Markov";
            if (Directory.Exists(markovPath) == false)
            {
                Directory.CreateDirectory(markovPath);
            }

            foreach (var guild in sender.Guilds)
            {
                string markovGuildPath = $"{AppDomain.CurrentDomain.BaseDirectory}\\Markov";
                if (Directory.Exists(markovGuildPath) == false)
                {
                    Directory.CreateDirectory(markovGuildPath);
                }

                var guildData = await jsonHandler.GetAllGuildDataFromJSON(guild.Key);
                if (guildData == null) guildData = new GuildData() {ID = guild.Key };

                Guilds.Add(guildData.ID, guildData);

                string path = $"{AppDomain.CurrentDomain.BaseDirectory}\\Markov\\{guildData.ID}\\chains.json";
                if (File.Exists(path))
                {
                    var markovWords = File.ReadAllText(path);
                    var guildMarkov = new MarkovChain() { GuildID = guildData.ID };
                    guildMarkov.ApplyWords(JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(markovWords));

                    markovChain.Add(guildData.ID, guildMarkov);
                }
                else
                {
                    markovChain.Add(guildData.ID, new MarkovChain() { GuildID = guildData.ID });
                }
            }
            await Task.Run(() => MarkovImage.InitMemes());

            OnServersListUpdate(discord);
            OnLog("Bot is connected and ready!");
            await CreatePaths();

            OnLog($"Сервера {discord.Guilds.Count}");
            //await ModifyBotNickname("Sneik");
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
            var guild = JSONDataHandler.DATA_TYPES.GUILD;
            var channel = JSONDataHandler.DATA_TYPES.CHANNEL;
            //var vc = JSONDataHandler.DATA_TYPES.USERVC;
            //var category = JSONDataHandler.DATA_TYPES.CATEGORY;
            //var role = JSONDataHandler.DATA_TYPES.ROLE;
            //var message = JSONDataHandler.DATA_TYPES.MESSAGE;

            var guildcategory = JSONDataHandler.DATA_CATEGORIES.GuildData;
            var channelcategory = JSONDataHandler.DATA_CATEGORIES.ChannelData;
            //var vccategory = JSONDataHandler.DATA_CATEGORIES.PrivateVCUserData;
            //var categorycategory = JSONDataHandler.DATA_CATEGORIES.CategoryData;
            //var rolecategory = JSONDataHandler.DATA_CATEGORIES.RoleData;
            //var messagecategory = JSONDataHandler.DATA_CATEGORIES.MessageData;

            jsonHandler.CreatePathIfNotExists(guild, guildcategory);
            jsonHandler.CreatePathIfNotExists(channel, channelcategory);
            //jsonHandler.CreatePathIfNotExists(vc, vccategory);
            //jsonHandler.CreatePathIfNotExists(category, categorycategory);
            //jsonHandler.CreatePathIfNotExists(role, rolecategory);
            //jsonHandler.CreatePathIfNotExists(message, messagecategory);

            await Task.CompletedTask;
        }

        private static async Task OnMessageCreated(DiscordClient sender, MessageCreateEventArgs e)
        {
            if (e.Author.IsBot) return;
            if (e.Channel.IsPrivate)
            {
                OnLog($"[PrivateChannel] {e.Author.Username}: {e.Message.Content}");

                if (e.Message.Content == "привет") await e.Message.RespondAsync("Приветствую!");
            }
            else
            {
                OnLog($"Server {e.Guild.Name} -> {e.Channel.Name} -> {e.Author.Username}: {e.Message.Content}");
            }
            
            // Сохраняем сообщения
            if (e.Channel.IsPrivate == false && e.Message.Content.StartsWith(Guilds[e.Guild.Id].Prefix) == false && Guilds[e.Guild.Id].MarkovReadingChannels.Contains(e.Channel.Id))
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
                }
                markovChain[e.Guild.Id].AddWords(words);
            }
            
            if ((e.Message.Content.ToLower().StartsWith("ping") && e.Channel.IsPrivate == true) || (e.Message.Content.ToLower().StartsWith("ping") && e.Message.MentionedUsers.Contains(discord.CurrentUser)))
            {
                await e.Message.RespondAsync(CreatePong("Pong!"));
            }else if (e.Message.Content.ToLower().Contains("sneik"))
            {
                await e.Message.ModifyAsync(e.Message.Content.ToLower().Replace("sneik", "||sneik||"));
            }else if ((e.Channel.IsPrivate == true && e.Message.Content.ToLower().Contains("покажи мордочку")) || (e.Message.MentionedUsers.Contains(discord.CurrentUser) && e.Message.Content.ToLower().Contains("покажи мордочку")))
            {
                var builder = new DiscordMessageBuilder()
                        .WithContent($"{e.Author.Mention} Вот он я")
                        .AddFile(new FileStream(@"C:\\Users\\super\\OneDrive\\Изображения\cartoon_boa_S652427499_St12_G7.5.jpeg", FileMode.Open));

                await e.Message.RespondAsync(builder);
            }
            else
            {
                if (e.Message.MentionedUsers.Contains(discord.CurrentUser) && Guilds[e.Guild.Id].MarkovWritingChannels.Contains(e.Channel.Id))
                {
                    string response = markovChain[e.Guild.Id].GenerateSentence(markovChain[e.Guild.Id].GetRandomStartWord(), new Random().Next(3,25));

                    if (new Random().Next(10) == 0)
                        response = response.ToUpper();

                    await e.Message.RespondAsync(response);
                }
            }
        }

        private static async Task ModifyBotNickname(string newNick, bool isEnd = false)
        {
            // Меняем ник бота обратно на оригинальный
            foreach (var guild in discord.Guilds)
            {
                var currentMember = await guild.Value.GetMemberAsync(discord.CurrentUser.Id);
                try
                {
                    await currentMember.ModifyAsync(x => x.Nickname = newNick);
                }
                catch (Exception e) { }
            }

            OnLog("Ник заменен");
            if (isEnd)
            {
                await discord.DisconnectAsync();
            }
        }
    }
}
