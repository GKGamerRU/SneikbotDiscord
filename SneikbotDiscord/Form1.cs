using DSharpPlus;
using SneikbotDiscord.BotConfig;
using SneikbotDiscord.Commands.Prefix;
using SneikbotDiscord.Sneik;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SneikbotDiscord
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Fun.ApplyAuto();

            LogButton.Click += delegate { tabControl1.SelectTab(0); };
            ServersButton.Click += delegate { tabControl1.SelectTab(1); };
            NeuralButton.Click += delegate { tabControl1.SelectTab(2); };
            AboutButton.Click += delegate { tabControl1.SelectTab(3); };
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //SneikBot.Stop();
            Task.Run(SneikBot.Stop).GetAwaiter().GetResult();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            SneikBot.OnLog += delegate (string message) {
                AddMessage(message);
            };
            SneikBot.OnServersListUpdate += delegate(DiscordClient client) { ServersLabel.Text = string.Join(Environment.NewLine, client.Guilds.Select(s=> s.Value.Name)); };

            SneikBot.botConfig = BotConfiguration.LoadConfig();
            IPAdressTextbox.Text = SneikBot.botConfig.NeuralAdresse;
            ModelNameTextBox.Text = SneikBot.botConfig.ModelProvider;
            SystemPromptTextbox.Text = SneikBot.botConfig.SystemPrompt;
            ProviderCombo.Text = SneikBot.botConfig.LocalNeuralProvider;
            Fun.ApplyLocalProvider();

            Task.Run(SneikBot.Start);
        }

        List<string> messages = new List<string>();
        void AddMessage(string message)
        {
            if (messages.Count >= 10)
            {
                messages.RemoveAt(0);
            }
            messages.Add(message);

            textBox1.Text = string.Join(Environment.NewLine, messages);

            // Прокручиваем вниз
            textBox1.SelectionStart = textBox1.Text.Length;
            textBox1.ScrollToCaret();
        }

        private void ApplyNeuralButton_Click(object sender, EventArgs e)
        {
            SneikBot.botConfig.NeuralAdresse = IPAdressTextbox.Text;
            SneikBot.botConfig.ModelProvider = ModelNameTextBox.Text;
            SneikBot.botConfig.SystemPrompt = SystemPromptTextbox.Text;
            SneikBot.botConfig.LocalNeuralProvider = ProviderCombo.Text;

            Fun.ApplyLocalProvider();
            BotConfiguration.SaveConfig(SneikBot.botConfig);
        }
    }
}
