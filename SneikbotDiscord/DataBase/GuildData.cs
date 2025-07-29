using SneikbotDiscord.Markov;
using System.Collections.Generic;

namespace SneikbotDiscord.DataBase
{
    public class GuildData
    {
        public ulong ID { get; set; }
        public string Prefix { get; set; } = "!";

        public List<ulong> MarkovReadingChannels = new List<ulong>();
        public List<ulong> MarkovWritingChannels = new List<ulong>();

        public MarkovConfiguration MarkovConfiguration { get; set; } = new MarkovConfiguration();
    }
}
