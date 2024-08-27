using SneikbotDiscord.Commands.Prefix;
using SneikbotDiscord.Sneik;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
            Task.Run(SneikBot.Stop).GetAwaiter().GetResult();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            SneikBot.OnLog += delegate (string message) { textBox1.Text += (Environment.NewLine + message); };
            Task.Run(SneikBot.Start);
        }
    }
}
