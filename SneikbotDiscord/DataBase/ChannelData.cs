using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneikbotDiscord.DataBase
{
    public class ChannelData
    {
        public ulong ID { get; set; }
        public ulong GuildID { get; set; }

        public ChannelData(ulong id, ulong guildID)
        {
            ID = id;
            GuildID = guildID;
        }
    }
}
