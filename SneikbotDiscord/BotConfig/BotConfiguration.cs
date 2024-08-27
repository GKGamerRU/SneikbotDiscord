using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SneikbotDiscord.BotConfig
{
    public class BotConfiguration
    {
        public string Token { get; set; }
        public string Prefix { get; set; }

        public static BotConfiguration LoadConfig()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
            var json = JsonConvert.DeserializeObject<BotConfiguration>(File.ReadAllText(path));
            return json;
        }
    }
}
