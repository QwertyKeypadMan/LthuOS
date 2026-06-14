using Cosmos.HAL;
using Cosmos.System;
using Cosmos.System.Graphics;
using CosmosMiniOS.Apps;
using CosmosMiniOS.Resources;
using CosmosMiniOS.System;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace CosmosMiniOS.Gui
{
    public static class Gui
    {
        public const int SW = 1280, SH = 720;
        public static Canvas Canvas;

        public static Bitmap Wallpaper, Cursor, AboutIcon, TestIcon, StartIcon;

        private static Cosmos.System.Graphics.Fonts.Font LthuFont;

        private static readonly Color CBar = Color.FromArgb(255, 30, 30, 30);
        private static readonly Color CButton = Color.FromArgb(255, 50, 90, 160);
        private static readonly Color CActive = Color.FromArgb(255, 60, 96, 150);
        private static readonly Color CText = Color.White;
        private static readonly Color CMenuItem = Color.FromArgb(255, 50, 50, 50);
        private static readonly Color CClock = Color.FromArgb(255, 45, 45, 45);
        private static readonly Color CErase = Color.FromArgb(255, 20, 20, 50);

        private static Pen _pBar, _pButton, _pActive, _pText, _pMenuItem, _pClock, _pErase;
        private static Pen _pFpsBg, _pFps;

        private static void DrawStringUnified(string text, int x, int y, Color color)
        {
            if (string.IsNullOrEmpty(text) || Canvas == null) return;

            var currentFont = LthuFont ?? Cosmos.System.Graphics.Fonts.PCScreenFont.Default;
            Canvas.DrawString(text, currentFont, new Pen(color), x, y);
        }

        private static bool _lastLeft;
        private static Process _drag;
        private static int _dragDX, _dragDY;
        private static GuiRect _prevDragRect;
        private static bool _fullRedraw = true;

        private const int TaskH = 48;
        private const int TaskY = SH - TaskH;
        private const int BtnX = 12, BtnY = TaskY + 8, BtnW = 92, BtnH = 32;

        private static string _clock = "";
        private static int _lastSec = -1;

        private static bool _menuOpen = false;
        private static GuiRect _menuRect;

        private class MenuItem
        {
            public GuiRect Rect;
            public string Text;
            public Action Click;
            public MenuItem(GuiRect r, string t, Action c) { Rect = r; Text = t; Click = c; }
        }
        private static List<MenuItem> _menuItems = new List<MenuItem>();

        private static int _lastCX = -1, _lastCY = -1;

        public static short Fps;
        private static short _fps2, _frames;

        public static void InitFonts()
        {
            try
            {
                LthuFont = Cosmos.System.Graphics.Fonts.PCScreenFont.Default;
            }
            catch
            {
                LthuFont = null;
            }
        }

        public static void StartGUI()
        {
            InitFonts();

            try { Canvas = new SVGAIICanvas(new Mode(SW, SH, ColorDepth.ColorDepth32)); }
            catch
            {
                try { Canvas = new VBECanvas(new Mode(SW, SH, ColorDepth.ColorDepth32)); }
                catch { Canvas = FullScreenCanvas.GetFullScreenCanvas(new Mode(SW, SH, ColorDepth.ColorDepth32)); }
            }

            _pBar = new Pen(CBar);
            _pButton = new Pen(CButton);
            _pActive = new Pen(CActive);
            _pText = new Pen(CText);
            _pMenuItem = new Pen(CMenuItem);
            _pClock = new Pen(CClock);
            _pErase = new Pen(CErase);

            _pFpsBg = new Pen(Color.FromArgb(255, 20, 20, 20));
            _pFps = new Pen(Color.LimeGreen);

            if (Files.CosmosMiniOSBackgroundRaw != null) Wallpaper = new Bitmap(Files.CosmosMiniOSBackgroundRaw);
            if (Files.CosmosMiniOSCursor != null) Cursor = new Bitmap(Files.CosmosMiniOSCursor);
            if (Files.AboutIcon != null) AboutIcon = new Bitmap(Files.AboutIcon);
            if (Files.testIcon != null) TestIcon = new Bitmap(Files.testIcon);
            if (Files.StartIcon != null) StartIcon = new Bitmap(Files.StartIcon);

            MouseManager.ScreenWidth = SW;
            MouseManager.ScreenHeight = SH;
            MouseManager.X = SW / 2;
            MouseManager.Y = SH / 2;

            BuildMenu();
            _fullRedraw = true;

            Canvas.DrawFilledRectangle(new Pen(Color.FromArgb(255, 255, 0, 0)), 0, 0, SW, SH);
            DrawStringUnified("GUI BASLADI", 100, 100, CText);
            Canvas.Display();
        }
        public static void Update()
        {
            if (_fps2 != RTC.Second) { Fps = _frames; _frames = 0; _fps2 = RTC.Second; }
            _frames++;

            HandleMouse();

            DrawWallpaper();
            DrawDesktopIcons();
            ProcessManager.Update();
            ProcessManager.DrawAll();
            DrawTaskbar();
            if (_menuOpen) DrawMenu();

            Canvas.DrawFilledRectangle(_pFpsBg, SW - 90, 10, 80, 25);

            var currentFont = LthuFont ?? Cosmos.System.Graphics.Fonts.PCScreenFont.Default;
            Canvas.DrawString("FPS: " + Fps.ToString(), currentFont, _pFps, SW - 85, 15);

            if (Cursor != null)
                Canvas.DrawImageAlpha(Cursor, (int)MouseManager.X, (int)MouseManager.Y);

            Canvas.Display();

            int s = DateTime.Now.Second;
            if (s != _lastSec)
            {
                _clock = DateTime.Now.ToString("HH:mm:ss");
                _lastSec = s;
                Cosmos.Core.Memory.Heap.Collect();
            }
        }

        private static void DrawWallpaper()
        {
            if (Wallpaper != null)
                Canvas.DrawImage(Wallpaper, 0, 0);
            else
                Canvas.DrawFilledRectangle(_pErase, 0, 0, SW, SH);
        }

        private static void EraseRect(GuiRect r)
        {
            int x = Math.Max(0, r.X);
            int y = Math.Max(0, r.Y);
            int w = Math.Min(r.W, SW - x);
            int h = Math.Min(r.H, SH - y);
            if (w <= 0 || h <= 0) return;
            Canvas.DrawFilledRectangle(_pErase, x, y, w, h);
        }

        private static void RedrawRegion(GuiRect region, Process skip)
        {
            for (int i = 0; i < ProcessManager.Count(); i++)
            {
                Process p = ProcessManager.GetAt(i);
                if (p == skip) continue;
                if (p.WindowData.WinPos.Intersects(region))
                    p.Draw();
            }
        }

        private static void DrawDesktopIcons()
        {
            if (AboutIcon != null) Canvas.DrawImage(AboutIcon, 48, 82, 48, 48);
            DrawStringUnified("About", 46, 134, CText);
        }

        private static void DrawTaskbar()
        {
            Canvas.DrawFilledRectangle(_pBar, 0, TaskY, SW, TaskH);
            Canvas.DrawFilledRectangle(_pButton, BtnX, BtnY, BtnW, BtnH);
            DrawStringUnified("Baslat", BtnX + 8, BtnY + 8, CText);

            int x = BtnX + BtnW + 16;
            for (int i = 0; i < ProcessManager.Count(); i++)
            {
                Process p = ProcessManager.GetAt(i);
                Canvas.DrawFilledRectangle(_pActive, x, TaskY + 8, 100, 32);
                string name = p.Name.Length > 10 ? p.Name.Substring(0, 10) : p.Name;
                DrawStringUnified(name, x + 8, TaskY + 16, CText);
                x += 110;
            }

            int cx = SW - 132;
            Canvas.DrawFilledRectangle(_pClock, cx, TaskY + 8, 120, 32);
            DrawStringUnified(_clock, cx + 8, TaskY + 16, CText);
        }

        private static void BuildMenu()
        {
            _menuItems.Clear();
            int mw = 200;
            int mx = BtnX;

            _menuItems.Add(new MenuItem(new GuiRect(0, 0, 0, 0), "Metin Editoru", () => OpenApp("Editor", new TextEditor())));
            _menuItems.Add(new MenuItem(new GuiRect(0, 0, 0, 0), "Komut Satiri", () => OpenApp("Terminal", new GuiShell())));
            _menuItems.Add(new MenuItem(new GuiRect(0, 0, 0, 0), "Dosya Yoneticisi", () => OpenApp("Files", new FileManager())));
            _menuItems.Add(new MenuItem(new GuiRect(0, 0, 0, 0), "About", () => OpenApp("About", new Messagebox())));
            _menuItems.Add(new MenuItem(new GuiRect(0, 0, 0, 0), "Kapat", () => { try { Cosmos.System.Power.Shutdown(); } catch { } }));

            int mh = 16 + _menuItems.Count * 32;
            int my = TaskY - mh - 4;
            _menuRect = new GuiRect(mx, my, mw, mh);

            for (int i = 0; i < _menuItems.Count; i++)
                _menuItems[i].Rect = new GuiRect(mx + 4, my + 8 + i * 32, mw - 8, 26);
        }

        private static void DrawMenu()
        {
            Canvas.DrawFilledRectangle(_pBar, _menuRect.X, _menuRect.Y, _menuRect.W, _menuRect.H);

            for (int i = 0; i < _menuItems.Count; i++)
            {
                var item = _menuItems[i];
                Canvas.DrawFilledRectangle(_pMenuItem, item.Rect.X, item.Rect.Y, item.Rect.W, item.Rect.H);
                DrawStringUnified(item.Text, item.Rect.X + 8, item.Rect.Y + 5, CText);
            }
        }

        private static void HandleMouse()
        {
            bool left = MouseManager.MouseState == MouseState.Left;
            int mx = (int)MouseManager.X;
            int my = (int)MouseManager.Y;

            if (left && _drag != null)
            {
                GuiRect r = _drag.WindowData.WinPos;
                r.X = Clamp(mx - _dragDX, 0, SW - r.W);
                r.Y = Clamp(my - _dragDY, 0, TaskY - r.H);
                _drag.WindowData.WinPos = r;
            }

            if (left && !_lastLeft)
            {
                if (Hit(mx, my, BtnX, BtnY, BtnW, BtnH))
                {
                    _menuOpen = !_menuOpen;
                    BuildMenu();
                    _fullRedraw = true;
                    _lastLeft = left;
                    return;
                }

                if (Hit(mx, my, 48, 82, 48, 48)) { OpenApp("About", new Messagebox()); _lastLeft = left; return; }

                if (_menuOpen)
                {
                    for (int i = 0; i < _menuItems.Count; i++)
                    {
                        var item = _menuItems[i];
                        if (item.Rect.Contains(mx, my))
                        {
                            try { item.Click?.Invoke(); } catch { }
                            _menuOpen = false;
                            _fullRedraw = true;
                            _lastLeft = left;
                            return;
                        }
                    }
                }

                if (!TryClose(mx, my))
                    TryDrag(mx, my);
            }
            else if (!left)
            {
                if (_drag != null) _fullRedraw = true;
                _drag = null;
            }

            _lastLeft = left;
        }

        private static bool TryClose(int mx, int my)
        {
            for (int i = ProcessManager.Count() - 1; i >= 0; i--)
            {
                Process p = ProcessManager.GetAt(i);
                if (Window.HitClose(p, mx, my))
                {
                    ProcessManager.Stop(p);
                    _drag = null;
                    _fullRedraw = true;
                    return true;
                }
            }
            return false;
        }

        private static void TryDrag(int mx, int my)
        {
            for (int i = ProcessManager.Count() - 1; i >= 0; i--)
            {
                Process p = ProcessManager.GetAt(i);
                if (Window.HitBar(p, mx, my))
                {
                    _drag = p;
                    _dragDX = mx - p.WindowData.WinPos.X;
                    _dragDY = my - p.WindowData.WinPos.Y;
                    _prevDragRect = p.WindowData.WinPos;
                    ProcessManager.BringToFront(p);
                    return;
                }
            }
        }

        private static void OpenApp(string name, Process p)
        {
            if (ProcessManager.HasProcess(name)) return;
            p.Name = name;
            p.WindowData.WinPos = new GuiRect(SW / 2 - 200, SH / 2 - 150, 400, 300);
            ProcessManager.Start(p);
            _menuOpen = false;
            _fullRedraw = true;
        }

        private static bool Hit(int px, int py, int x, int y, int w, int h)
            => px >= x && px <= x + w && py >= y && py <= y + h;

        private static int Clamp(int v, int lo, int hi)
        {
            if (v < lo) return lo;
            if (v > hi) return hi;
            return v;
        }
    }
}