using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneikbotDiscord.DataBase
{
    public class GuildFilesManager
    {
        public static int GetImagesCount(ulong guildId) 
        {
            string path = $"{AppDomain.CurrentDomain.BaseDirectory}\\GuildsFiles\\{guildId}\\Images";
            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            }

            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            return directoryInfo.GetFiles().Length;
        }
        public static string GetRandomImage(ulong guildId) 
        {
            string path = $"{AppDomain.CurrentDomain.BaseDirectory}\\GuildsFiles\\{guildId}\\Images";
            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            }

            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            var files = directoryInfo.GetFiles();

            if (files.Length == 0) return null;

            return files[new Random().Next(0, files.Length)].FullName;
        }
        public static void SaveImage(ulong guildId, Bitmap bitmap, string fileName)
        {
            string path = $"{AppDomain.CurrentDomain.BaseDirectory}\\GuildsFiles\\{guildId}\\Images";
            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            }

            if (File.Exists(Path.Combine(path, fileName))) return;

            string filePath = Path.Combine(path, fileName);
            bitmap.Save(filePath);
        }
    }
}
