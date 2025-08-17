using SneikbotDiscord.Commands.Prefix;
using SneikbotDiscord.Sneik;
using System;
using System.Collections.Generic;
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
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //SneikBot.Stop();
            //Task.Run(SneikBot.Stop).GetAwaiter().GetResult();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            SneikBot.OnLog += delegate (string message) {
                AddMessage(message);
            };
            //Task.Run(SneikBot.Start);
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
    }
}
