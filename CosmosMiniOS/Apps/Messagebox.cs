using Cosmos.System.Graphics;
using Cosmos.System.Graphics.Fonts;
using CosmosMiniOS.Gui;
using CosmosMiniOS.System;
using System.Drawing;

namespace CosmosMiniOS.Apps
{
    public class Messagebox : Process
    {
        private static readonly Pen _body = new Pen(Color.FromArgb(255, 45, 45, 55));
        private static readonly Pen _text = new Pen(Color.White);

        public override void Run() { }

        public override void Draw()
        {
            Window.DrawTop(this, Gui.Gui.Canvas);
            Window.DrawBody(WindowData.WinPos, Color.FromArgb(255, 45, 45, 55), Gui.Gui.Canvas);

            int x = WindowData.WinPos.X + 16;
            int y = WindowData.WinPos.Y + Window.TopSize + 16;
            Gui.Gui.Canvas.DrawString("LthuOS 1.1", PCScreenFont.Default, _text, x, y);
            Gui.Gui.Canvas.DrawString("Window system working", PCScreenFont.Default, _text, x, y + 24);
        }
    }
}