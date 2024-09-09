using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneikbotDiscord.DataBase
{
    public class GuildData
    {
        public ulong ID { get; set; }

        public string Prefix { get; set; }

        //public List<ulong> MarkovReadingChannels = new List<ulong>();
        //public List<ulong> MarkovWritingChannels = new List<ulong>();

        //public Dictionary<string, List<string>> chain = new Dictionary<string, List<string>>();

        public GuildData(ulong id, string prefix)
        {
            ID = id;
            Prefix = prefix;
        }
    }
}
