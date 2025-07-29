using System;
using System.Windows.Forms;

namespace SneikbotDiscord
{
    public class Program
    {/// <summary>
     /// Главная точка входа для приложения.
     /// </summary>
     /// 
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
