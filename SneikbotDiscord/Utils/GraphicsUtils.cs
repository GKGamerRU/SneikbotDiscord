using System.Drawing.Drawing2D;
using System.Drawing;

namespace SneikbotDiscord.Utils
{
    public static class GraphicsUtils
    {
        public static void DrawOutlinedText(Graphics g, string text, Font font, Color textColor, Color outlineColor, PointF position, SizeF size, StringFormat stringFormat)
        {
            // Толщина обводки
            float outlineThickness = 3f;

            // Рисуем обводку
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddString(text, font.FontFamily, (int)font.Style, font.Size, new RectangleF(position, size), stringFormat);

                using (Pen outlinePen = new Pen(outlineColor, outlineThickness) { LineJoin = System.Drawing.Drawing2D.LineJoin.Round })
                {
                    g.DrawPath(outlinePen, path);
                }

                // Рисуем текст
                using (Brush textBrush = new SolidBrush(textColor))
                {
                    g.FillPath(textBrush, path);
                }
            }
        }
    }
}
