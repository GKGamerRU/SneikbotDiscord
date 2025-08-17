using Newtonsoft.Json;
using System;
using System.IO;
using System.Windows;

namespace SneikbotDiscord.BotConfig
{
    public class BotConfiguration
    {
        public string Token { get; set; }
        public string Prefix { get; set; }

        public string NeuralAdresse { get; set; } = "http://localhost:11434";
        public string LocalNeuralProvider { get; set; } = "ollama";
        public string ModelProvider { get; set; } = "qwen3:8b";
        public string SystemPrompt { get; set; } = "Ты дружелюбный и полезный локальный ассистент. Отвечай кратко и по делу. Используй Markdown для форматирования и оформляй код в блоки с тройными обратными апострофами для общения в discord.";

        public static void SaveConfig(BotConfiguration configuration) { 
            var json = JsonConvert.SerializeObject(configuration);

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
            File.WriteAllText(path, json);
        }

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
