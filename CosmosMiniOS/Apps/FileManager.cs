using Cosmos.HAL; // RTC saat birimi için gerekli reis
using Cosmos.System;
using Cosmos.System.Graphics;
using Cosmos.System.Graphics.Fonts;
using CosmosMiniOS.Gui;
using CosmosMiniOS.System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace CosmosMiniOS.Apps
{
    public class FileManager : Process
    {
        private string _currentPath = @"0:\";
        private List<string> _items = new List<string>();
        private int _selectedIndex = -1;

        // Çift tıklama tespiti için sayaçlar
        private uint _lastClickTime = 0;
        private int _lastClickedIndex = -2;
        private const uint DoubleClickDelay = 500;

        // RAM dostu statik kalemler (Messagebox'taki gibi)
        private static readonly Pen _textPen = new Pen(Color.White);
        private static readonly Pen _selectedPen = new Pen(Color.FromArgb(255, 50, 90, 160));
        private static readonly Pen _dirPen = new Pen(Color.FromArgb(255, 240, 190, 70)); // Klasörler sarı

        private const int ItemHeight = 20;

        public FileManager()
        {
            // İlk başta Name vermiyoruz, OpenApp metodun zaten ezerek "Files" yapıyor reis.
            RefreshDirectory();
        }

        private void RefreshDirectory()
        {
            try
            {
                _items.Clear();
                _selectedIndex = -1;

                if (_currentPath != @"0:\")
                {
                    _items.Add("[ .. ] Ust Klasor");
                }

                foreach (var dir in Directory.GetDirectories(_currentPath))
                {
                    _items.Add("<DIR> " + Path.GetFileName(dir));
                }

                foreach (var file in Directory.GetFiles(_currentPath))
                {
                    _items.Add("      " + Path.GetFileName(file));
                }
            }
            catch
            {
                _items.Clear();
                _items.Add("Surucu Hazir Degil!");
            }
        }

        // 🚀 Senin mimaride döngü Run() üzerinden dönüyor olabilir, boş geçmeyelim
        public override void Run()
        {
            HandleKeyboard();
            HandleMouseClick();
        }

        public override void Draw()
        {
            // 1. Senin pencere sisteminin üst çubuğunu ve sınırlarını çiziyoruz
            Window.DrawTop(this, Gui.Gui.Canvas);

            // 2. Senin Window.DrawBody metodunu kullanarak arka planı tam kapatıyoruz
            Window.DrawBody(WindowData.WinPos, Color.FromArgb(255, 35, 35, 35), Gui.Gui.Canvas);

            int wx = WindowData.WinPos.X;
            int wy = WindowData.WinPos.Y;
            int ww = WindowData.WinPos.W;
            int wh = WindowData.WinPos.H;

            // Yazıların başlık çubuğuna (TopSize) çarpmaması için başlangıç noktası
            int startY = wy + Window.TopSize;

            // 3. Mevcut Konumu Yazdır
            Gui.Gui.Canvas.DrawString(" Konum: " + _currentPath, PCScreenFont.Default, _textPen, wx + 12, startY + 8);

            int itemX = wx + 12;
            int itemY = startY + 32;
            int itemW = ww - 24;

            // 4. Dosya ve Klasörleri Listele
            for (int i = 0; i < _items.Count; i++)
            {
                // Pencerenin altından dışarı taşmayı engelle
                if (itemY + ItemHeight > wy + wh - 8) break;

                // Seçili olan satırın arkasını boya
                if (i == _selectedIndex)
                {
                    Gui.Gui.Canvas.DrawFilledRectangle(_selectedPen, itemX, itemY, itemW, ItemHeight);
                }

                // Klasörleri sarı, dosyaları beyaz çiz
                Pen currentPen = _textPen;
                if (_items[i].StartsWith("<DIR>") || _items[i].StartsWith("[ .."))
                {
                    currentPen = _dirPen;
                }

                Gui.Gui.Canvas.DrawString(_items[i], PCScreenFont.Default, currentPen, itemX + 6, itemY + 2);
                itemY += ItemHeight;
            }
        }

        private void HandleKeyboard()
        {
            if (KeyboardManager.TryReadKey(out KeyEvent keyEvent))
            {
                if (keyEvent.Key == ConsoleKeyEx.DownArrow)
                {
                    if (_selectedIndex < _items.Count - 1) _selectedIndex++;
                }
                else if (keyEvent.Key == ConsoleKeyEx.UpArrow)
                {
                    if (_selectedIndex > 0) _selectedIndex--;
                }
                else if (keyEvent.Key == ConsoleKeyEx.Enter)
                {
                    ExecuteSelection();
                }
            }
        }

        private void HandleMouseClick()
        {
            if (MouseManager.MouseState == MouseState.Left)
            {
                int mx = (int)MouseManager.X;
                int my = (int)MouseManager.Y;

                int wx = WindowData.WinPos.X;
                int wy = WindowData.WinPos.Y;
                int ww = WindowData.WinPos.W;

                int startY = wy + Window.TopSize + 32;

                if (mx >= wx + 12 && mx <= wx + ww - 12 && my >= startY)
                {
                    int clickedIndex = (my - startY) / ItemHeight;

                    if (clickedIndex >= 0 && clickedIndex < _items.Count)
                    {
                        uint currentTime = (uint)RTC.Second * 1000 + (uint)RTC.Minute * 60000;

                        // Çift tıklama kontrolü
                        if (clickedIndex == _lastClickedIndex && (currentTime - _lastClickTime) < DoubleClickDelay)
                        {
                            _selectedIndex = clickedIndex;
                            ExecuteSelection();
                            _lastClickedIndex = -2;
                        }
                        else
                        {
                            _selectedIndex = clickedIndex;
                            _lastClickedIndex = clickedIndex;
                            _lastClickTime = currentTime;
                        }

                        // Mouse bırakılana kadar kilitle ki sapıtmasın
                        while (MouseManager.MouseState == MouseState.Left) { }
                    }
                }
            }
        }

        private void ExecuteSelection()
        {
            if (_selectedIndex < 0 || _selectedIndex >= _items.Count) return;

            string selected = _items[_selectedIndex];

            try
            {
                if (selected == "[ .. ] Ust Klasor")
                {
                    var parent = Directory.GetParent(_currentPath);
                    if (parent != null)
                    {
                        _currentPath = parent.FullName;
                        if (!_currentPath.EndsWith(@"\")) _currentPath += @"\";
                    }
                    RefreshDirectory();
                }
                else if (selected.StartsWith("<DIR>"))
                {
                    string folderName = selected.Substring(6);
                    _currentPath = Path.Combine(_currentPath, folderName) + @"\";
                    RefreshDirectory();
                }
            }
            catch
            {
                RefreshDirectory();
            }
        }
    }
}