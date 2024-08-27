using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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
            if (File.Exists(path))
            {
                var json = JsonConvert.DeserializeObject<BotConfiguration>(File.ReadAllText(path));
                return json;
            }
            else
            {
                var newConfig = new BotConfiguration();
                newConfig.Token = "YOUR_BOT_TOKEN_HERE";
                newConfig.Prefix = "!";
                MessageBox.Show("Файл конфигурации config.json не найден, но был создан. Пожалуйста, Введите данные в этот файл.");

                File.WriteAllText(path, JsonConvert.SerializeObject(newConfig));
                return newConfig;
            }
        }
    }
}
