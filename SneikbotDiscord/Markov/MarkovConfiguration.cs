using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using SneikbotDiscord.Sneik;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneikbotDiscord.Markov
{
    public class MarkovConfiguration
    {
        public int WordsInKey { get; set; } = 2;
        public bool isRandomCut { get; set; } = false;
        public bool isSuperMarkov { get; set; } = true;

        public bool TryExecuteOption(CommandContext ctx, string command, string value, out string message)
        {
            switch (command.ToLower())
            {
                case "supermarkovmode":
                    if (bool.TryParse(value.ToLower(), out bool isSuperMode))
                    {
                        SneikBot.Guilds[ctx.Guild.Id].MarkovConfiguration.isSuperMarkov = isSuperMode;
                        message = $"Опция SuperMarkovMode успешно изменена на {isSuperMode}";
                        return true;
                    }
                    else
                    {
                        message = $"Неверный формат, требуется true или false";
                        return false;
                    }
                case "israndomcut":
                    if (bool.TryParse(value.ToLower(), out bool isRandomCut))
                    {
                        SneikBot.Guilds[ctx.Guild.Id].MarkovConfiguration.isRandomCut = isRandomCut; 
                        message = $"Опция IsRandomCut успешно изменена на {isRandomCut}";
                        return true;
                    }
                    else
                    {
                        message = $"Неверный формат, требуется true или false";
                        return false;
                    }
                case "wordsinkey":
                    if (int.TryParse(value, out int wordsInKey) && wordsInKey >= 2 && wordsInKey <= 3)
                    {
                        SneikBot.Guilds[ctx.Guild.Id].MarkovConfiguration.WordsInKey = wordsInKey;
                        message = $"Опция WordsInKey успешно изменена на {wordsInKey}";
                        return true;
                    }
                    else
                    {
                        message = $"Неверный формат, требуется цифра 2 или 3";
                        return false;
                    }
                default:
                    message = $"Неверный формат, такой опции не существует.";
                    return false;
            }
        }
    }
}
