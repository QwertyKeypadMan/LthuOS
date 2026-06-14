using Cosmos.System.Graphics;
using Cosmos.System.Graphics.Fonts;
using CosmosMiniOS.System;
using System.Drawing;

namespace CosmosMiniOS.Gui
{
    public static class Window
    {
        public const int TopSize = 28;
        public const int BorderSize = 1;

        private static readonly Color ColBarAct = Color.FromArgb(255, 60, 96, 150);
        private static readonly Color ColClose = Color.FromArgb(255, 200, 50, 50);
        private static readonly Color ColText = Color.White;
        private static readonly Color ColBorder = Color.FromArgb(255, 80, 80, 80);

        // Canvas base class kullan — SVGAIICanvas değil
        public static void DrawTop(Process p, Canvas canvas)
        {
            GuiRect r = p.WindowData.WinPos;


            canvas.DrawFilledRectangle(new Pen(ColBarAct), r.X, r.Y, r.W, TopSize);
            canvas.DrawString(p.Name, PCScreenFont.Default, new Pen(ColText), r.X + 8, r.Y + 7);

            // Kapat butonu
            canvas.DrawFilledRectangle(new Pen(ColClose), r.Right - 22, r.Y + 5, 16, 18);
            canvas.DrawString("x", PCScreenFont.Default, new Pen(ColText), r.Right - 19, r.Y + 7);

            // Kenar
            canvas.DrawRectangle(new Pen(ColBorder), r.X, r.Y, r.W, r.H);
        }

        public static void DrawBody(GuiRect r, Color bodyColor, Canvas canvas)
        {
            canvas.DrawFilledRectangle(new Pen(bodyColor),
                r.X, r.Y + TopSize, r.W, r.H - TopSize);
        }

        public static bool HitClose(Process p, int mx, int my)
        {
            GuiRect r = p.WindowData.WinPos;
            return mx >= r.Right - 22 && mx <= r.Right - 6
                && my >= r.Y + 5 && my <= r.Y + 23;
        }

        public static bool HitBar(Process p, int mx, int my)
        {
            GuiRect r = p.WindowData.WinPos;
            return mx >= r.X && mx <= r.Right - 24
                && my >= r.Y && my <= r.Y + TopSize;
        }
    }
}