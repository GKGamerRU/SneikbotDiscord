using DSharpPlus.Entities;
using SneikbotDiscord.Utils;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace SneikbotDiscord.Markov
{
    public class MarkovImage
    {
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

        static Pen demotivatorFrame = new Pen(Brushes.White, 2);

        public static Bitmap GenerateDemotivator(DiscordGuild guild, MarkovChain chain, Bitmap bitmap = null)
        {
            string response = chain.GenerateSentence(chain.GetRandomStartWord(), new Random().Next(3, 25)).RemoveSentenceEnds();
            string response2 = chain.GenerateSentence(chain.GetRandomStartWord(), new Random().Next(3, 25));

            if (new Random().Next(10) == 0)
                response = response.ToUpper();
            if (new Random().Next(10) == 0)
                response = response.ToUpper();

            var members = guild.GetAllMembersAsync().GetAwaiter().GetResult().Where(mem => mem.AvatarUrl != null);
            DiscordMember member = guild.GetAllMembersAsync().GetAwaiter().GetResult().ElementAt(new Random().Next(members.Count()));

            string avatarUrl = member.AvatarUrl;

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

            var members = guild.GetAllMembersAsync().GetAwaiter().GetResult().Where(mem => mem.AvatarUrl != null);
            DiscordMember member = guild.GetAllMembersAsync().GetAwaiter().GetResult().ElementAt(new Random().Next(members.Count()));

            string avatarUrl = member.AvatarUrl;

            Bitmap picture;
            if(bitmap == null)
            {
                picture = new Bitmap(512, 512);
                foreFont = new Font("Segoe UI", 24, FontStyle.Bold);
            }
            else
            {
                var factor = (float)bitmap.Width / bitmap.Height;
                picture = new Bitmap((int)(512 * factor), 512);

                foreFont = new Font("Segoe UI", (picture.Height / 21) >= 8 ? picture.Height / 21 : 8, FontStyle.Bold);
            }

            Graphics gfx = Graphics.FromImage(picture);
            gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            gfx.SmoothingMode = SmoothingMode.AntiAlias;
            
            if (bitmap == null)
            {
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
    }
}
