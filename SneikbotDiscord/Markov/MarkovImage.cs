using DSharpPlus.Entities;
using SneikbotDiscord.Properties;
using SneikbotDiscord.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;

namespace SneikbotDiscord.Markov
{
    public class MarkovImage
    {
        static List<Bitmap> Memes = new List<Bitmap>();
        public static void InitMemes()
        {
            string path = $"{AppDomain.CurrentDomain.BaseDirectory}\\Memes";
            if(Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            }

            DirectoryInfo directoryInfo = new DirectoryInfo(path);

            foreach (var meme in directoryInfo.EnumerateFiles()) {
                Bitmap temp = new Bitmap(meme.FullName);
                float factorX = (float)temp.Height / temp.Width;
                //SizeF newSize = new SizeF(temp.Height / factor, temp.Width / factor);
                SizeF newSize = new SizeF(170 / factorX, 170);

                Memes.Add(new Bitmap(temp,newSize.ToSize()));
            }
        }


        static StringFormat StringFormat = new StringFormat()
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Far,
        };
        static StringFormat CenterStringFormat = new StringFormat()
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
        };
        static Font foreFont = new Font("Segoe UI", 24, FontStyle.Bold);

        static Font DemotivatorForeFont = new Font("Times New Roman", 24, FontStyle.Bold);
        static Font DemotivatorBackFont = new Font("Microsoft Sans Serif", 16, FontStyle.Regular);

        static Font JacqueSentense = new Font("Microsoft Sans Serif", 22);
        static Font ComicMeme = new Font("Georgia", 13);

        static Pen demotivatorFrame = new Pen(Brushes.White, 2);

        public static Bitmap GenerateDemotivator(DiscordGuild guild, MarkovChain chain, Bitmap bitmap = null)
        {
            string response = chain.GenerateSentence(chain.GetRandomStartWord(), new Random().Next(3, 25)).RemoveSentenceEnds();
            string response2 = chain.GenerateSentence(chain.GetRandomStartWord(), new Random().Next(3, 25));

            if (new Random().Next(10) == 0)
                response = response.ToUpper();

            Bitmap picture = new Bitmap(768, 490);
            var rectangle = new Rectangle(80, 30, 607, 350);
            var imageRect = new Rectangle(90, 40, 587, 330);

            Graphics gfx = Graphics.FromImage(picture);
            gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            gfx.SmoothingMode = SmoothingMode.AntiAlias;
            gfx.CompositingQuality = CompositingQuality.HighQuality;

            gfx.Clear(Color.Black);

            if (bitmap == null)
            {
                var members = guild.GetAllMembersAsync().GetAwaiter().GetResult().Where(mem => mem.AvatarUrl != null);
                DiscordMember member = guild.GetAllMembersAsync().GetAwaiter().GetResult().ElementAt(new Random().Next(members.Count()));

                string avatarUrl = member.AvatarUrl;

                Bitmap avatarBitmap = ImageUtils.GetImageFromUrlAsync(avatarUrl).GetAwaiter().GetResult();
                gfx.DrawImage(avatarBitmap, imageRect);
            }
            else
            {
                gfx.DrawImage(bitmap, imageRect);
            }
            gfx.DrawRectangle(demotivatorFrame, rectangle);

            GraphicsUtils.DrawOutlinedText(gfx, response, DemotivatorForeFont, Color.White, Color.Black, new Point(0,390), new Size(768, 50), CenterStringFormat);
            GraphicsUtils.DrawOutlinedText(gfx, response2, DemotivatorBackFont, Color.White, Color.Black, new Point(0, 440), new Size(768, 50),CenterStringFormat);
            return picture;
        }
        public static Bitmap GenerateBruh(DiscordGuild guild, MarkovChain chain, Bitmap bitmap = null)
        {
            string response = chain.GenerateSentence(chain.GetRandomStartWord(), new Random().Next(3, 25));

            if (new Random().Next(10) == 0)
                response = response.ToUpper();

            Bitmap picture;
            if(bitmap == null)
            {
                picture = new Bitmap(512, 512);
                foreFont = new Font("Segoe UI", 28, FontStyle.Bold);
            }
            else
            {
                var factor = (float)bitmap.Width / bitmap.Height;
                picture = new Bitmap((int)(512 * factor), 512);

                foreFont = new Font("Segoe UI", (picture.Height / 18) >= 12 ? picture.Height / 21 : 12, FontStyle.Bold);
            }

            Graphics gfx = Graphics.FromImage(picture);
            gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            gfx.SmoothingMode = SmoothingMode.AntiAlias;
            
            if (bitmap == null)
            {
                var members = guild.GetAllMembersAsync().GetAwaiter().GetResult().Where(mem => mem.AvatarUrl != null);
                DiscordMember member = guild.GetAllMembersAsync().GetAwaiter().GetResult().ElementAt(new Random().Next(members.Count()));

                string avatarUrl = member.AvatarUrl;

                Bitmap avatarBitmap = ImageUtils.GetImageFromUrlAsync(avatarUrl).GetAwaiter().GetResult(); 
                gfx.DrawImage(avatarBitmap, gfx.VisibleClipBounds);
            }
            else
            {
                gfx.DrawImage(bitmap, gfx.VisibleClipBounds);
            }

            // Положение текста: по центру по горизонтали и внизу по вертикали
            PointF position = new PointF(0, 0);
            GraphicsUtils.DrawOutlinedText(gfx, response, foreFont, Color.White, Color.Black, position, picture.Size,StringFormat);

            return picture;
        }

        public static Bitmap GenerateJacque(DiscordGuild guild, MarkovChain chain)
        {
            string response = chain.GenerateSentence(chain.GetRandomStartWord(), new Random().Next(3, 25));

            if (new Random().Next(10) == 0)
                response = response.ToUpper();

            Bitmap picture = new Bitmap(Resources.Jacque);

            Graphics gfx = Graphics.FromImage(picture);
            gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            gfx.SmoothingMode = SmoothingMode.AntiAlias;

            // Положение текста: по центру по горизонтали и внизу по вертикали
            PointF position = new PointF(375, 100);
            GraphicsUtils.DrawOutlinedText(gfx, response, JacqueSentense, Color.Black, Color.White, position, new SizeF(400,picture.Height - 200), CenterStringFormat);

            return picture;
        }

        public static Bitmap GenerateComics(DiscordGuild guild, MarkovChain chain)
        {
            //string response = chain.GenerateSentence(chain.GetRandomStartWord(), new Random().Next(3, 25));

            //if (new Random().Next(10) == 0)
            //    response = response.ToUpper();

            Random random = new Random();
            int count = random.Next(1, 5);
            if (count == 3) count = 4;

            Bitmap MainPage;
            if(count == 4)
            {
                MainPage = new Bitmap(640, 480, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            }else if (count == 2)
            {
                MainPage = new Bitmap(640, 240, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            }
            else
            {
                MainPage = new Bitmap(320, 240, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            }

            Graphics gfx = Graphics.FromImage(MainPage);
            gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            gfx.SmoothingMode = SmoothingMode.AntiAlias;

            gfx.Clear(Color.White);

            Bitmap[] memes = new Bitmap[4];
            for (int i = 0; i < count; i++)
            {
                string response = chain.GenerateSentence(chain.GetRandomStartWord(), new Random().Next(3, 25));
                if (new Random().Next(10) == 0)
                    response = response.ToUpper();

                Bitmap meme = Memes[random.Next(Memes.Count)];
                Bitmap memFrame = new Bitmap(320, 240, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

                Graphics gfx2 = Graphics.FromImage(memFrame);
                gfx2.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                gfx2.SmoothingMode = SmoothingMode.AntiAlias;

                GraphicsUtils.DrawOutlinedText(gfx2, response, ComicMeme, Color.Black, Color.White, Point.Empty, new SizeF(320, 70), CenterStringFormat);
                gfx2.DrawImage(meme, new Rectangle(random.Next(memFrame.Width - meme.Width), 240 - meme.Height, meme.Width, meme.Height));
                memes[i] = memFrame;
            }

            if (memes[0] != null) gfx.DrawImage(memes[0], new Point(0, 0));
            if (memes[1] != null) gfx.DrawImage(memes[1], new Point(320, 0));
            if (memes[2] != null) gfx.DrawImage(memes[2], new Point(0, 240));
            if (memes[3] != null) gfx.DrawImage(memes[3], new Point(320, 240));

            if(count > 1)gfx.DrawLine(Pens.Gray, MainPage.Width / 2, 0, MainPage.Width / 2, MainPage.Height);
            if(count > 3) gfx.DrawLine(Pens.Gray, 0, MainPage.Height / 2, MainPage.Width, MainPage.Height / 2);
            // Положение текста: по центру по горизонтали и внизу по вертикали
            //GraphicsUtils.DrawOutlinedText(gfx, response, JacqueSentense, Color.Black, Color.White, position, new SizeF(400, picture.Height - 200), CenterStringFormat);

            return MainPage;
        }
    }
}
