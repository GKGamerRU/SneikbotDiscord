using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SneikbotDiscord.Utils
{
    public static class ImageUtils
    {
        public static async Task<Bitmap> GetImageFromUrlAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                var stream = await client.GetStreamAsync(url);
                return new Bitmap(stream);
            }
        }
        public static bool IsImage(string fileName)
        {
            string[] validExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            string fileExtension = System.IO.Path.GetExtension(fileName).ToLower();
            return Array.Exists(validExtensions, ext => ext == fileExtension);
        }
    }
}
