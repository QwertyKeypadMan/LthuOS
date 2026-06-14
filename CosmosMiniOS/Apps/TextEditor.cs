using Cosmos.System;
using Cosmos.System.Graphics;
using Cosmos.System.Graphics.Fonts;
using CosmosMiniOS.Gui;
using CosmosMiniOS.System;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace CosmosMiniOS.Apps
{
    public class TextEditor : Process
    {
        private List<string> _lines = new List<string>();
        private int _currentLineIndex = 0;
        private string _fileName = "notlar.txt";
        private string _statusMessage = "Hazir.";

        // Menü Kontrol Değişkenleri
        private bool _isMenuOpen = false;
        private GuiRect _fileMenuBtnRect;
        private GuiRect _menuDropdownRect;

        // Alt Menü Seçenekleri
        private GuiRect _btnNewRect;
        private GuiRect _btnOpenRect;
        private GuiRect _btnSaveRect;

        // Donmayı engelleyen akıllı fare durum kontrolü
        private bool _wasPressed = false;

        // Çizim Kalemleri (RAM Dostu Statik Yapı)
        private static readonly Pen _textPen = new Pen(Color.White);
        private static readonly Pen _menuBarBg = new Pen(Color.FromArgb(255, 45, 45, 50));
        private static readonly Pen _menuDropdownBg = new Pen(Color.FromArgb(255, 55, 55, 60));
        private static readonly Pen _menuHoverPen = new Pen(Color.FromArgb(255, 70, 110, 180));
        private static readonly Pen _statusBgPen = new Pen(Color.FromArgb(255, 25, 25, 25));
        private static readonly Pen _statusTextPen = new Pen(Color.FromArgb(255, 0, 255, 150));
        private static readonly Pen _cursorPen = new Pen(Color.Cyan);

        private const int LineHeight = 18;
        private const int MenuBarHeight = 22;

        public TextEditor()
        {
            _lines.Add("");
        }

        public override void Run()
        {
            HandleKeyboard();
            HandleMouseClick();
        }

        public override void Draw()
        {
            // 1. Senin pencerendeki üst mavi çubuğu çiziyoruz
            Window.DrawTop(this, Gui.Gui.Canvas);

            // 2. Ana Gövde Arka Planını Çiz
            Window.DrawBody(WindowData.WinPos, Color.FromArgb(255, 30, 30, 35), Gui.Gui.Canvas);

            int wx = WindowData.WinPos.X;
            int wy = WindowData.WinPos.Y;
            int ww = WindowData.WinPos.W;
            int wh = WindowData.WinPos.H;

            int editorTopY = wy + Window.TopSize;
            int menuBarY = editorTopY;
            int textStartY = menuBarY + MenuBarHeight + 8;

            // Alanları pencerenin o anki konumuna göre güncelle (Sürüklenirse patlamasın reis)
            _fileMenuBtnRect = new GuiRect(wx + 5, menuBarY + 2, 60, 18);
            _menuDropdownRect = new GuiRect(wx + 5, menuBarY + MenuBarHeight, 100, 70);

            _btnNewRect = new GuiRect(_menuDropdownRect.X, _menuDropdownRect.Y + 2, _menuDropdownRect.W, 20);
            _btnOpenRect = new GuiRect(_menuDropdownRect.X, _menuDropdownRect.Y + 22, _menuDropdownRect.W, 20);
            _btnSaveRect = new GuiRect(_menuDropdownRect.X, _menuDropdownRect.Y + 42, _menuDropdownRect.W, 20);

            // 3. Menü Çubuğunu Çiz
            Gui.Gui.Canvas.DrawFilledRectangle(_menuBarBg, wx + 1, menuBarY, ww - 2, MenuBarHeight);

            int mx = (int)MouseManager.X;
            int my = (int)MouseManager.Y;
            if (mx >= _fileMenuBtnRect.X && mx <= _fileMenuBtnRect.X + _fileMenuBtnRect.W &&
                my >= _fileMenuBtnRect.Y && my <= _fileMenuBtnRect.Y + _fileMenuBtnRect.H)
            {
                Gui.Gui.Canvas.DrawFilledRectangle(_menuHoverPen, _fileMenuBtnRect.X, _fileMenuBtnRect.Y, _fileMenuBtnRect.W, _fileMenuBtnRect.H);
            }
            Gui.Gui.Canvas.DrawString(" Dosya", PCScreenFont.Default, _textPen, wx + 8, menuBarY + 4);

            // 4. Metin Çatısını ve Satırları Çiz
            int currentY = textStartY;
            for (int i = 0; i < _lines.Count; i++)
            {
                if (currentY + LineHeight > (wy + wh) - 25) break;

                Gui.Gui.Canvas.DrawString(_lines[i], PCScreenFont.Default, _textPen, wx + 12, currentY);

                // Menü kapalıysa imleci yanıp söndür
                if (i == _currentLineIndex && !_isMenuOpen)
                {
                    int cursorX = wx + 12 + (_lines[i].Length * 8);
                    Gui.Gui.Canvas.DrawFilledRectangle(_cursorPen, cursorX, currentY + 2, 2, 14);
                }
                currentY += LineHeight;
            }

            // 5. Alt Durum Çubuğunu Çiz (Status Bar)
            int statusY = wy + wh - 22;
            Gui.Gui.Canvas.DrawFilledRectangle(_statusBgPen, wx + 1, statusY, ww - 2, 21);
            string infoText = $" Dosya: {_fileName} | {_statusMessage}";
            Gui.Gui.Canvas.DrawString(infoText, PCScreenFont.Default, _statusTextPen, wx + 8, statusY + 4);

            // 6. Menü Açıksa Dropdown Listesini En Üste Bindir
            if (_isMenuOpen)
            {
                Gui.Gui.Canvas.DrawFilledRectangle(_menuDropdownBg, _menuDropdownRect.X, _menuDropdownRect.Y, _menuDropdownRect.W, _menuDropdownRect.H);
                Gui.Gui.Canvas.DrawRectangle(_textPen, _menuDropdownRect.X, _menuDropdownRect.Y, _menuDropdownRect.W, _menuDropdownRect.H);

                DrawMenuButton(" [ ] Yeni", _btnNewRect, mx, my);
                DrawMenuButton(" [^] Ac", _btnOpenRect, mx, my);
                DrawMenuButton(" [*] Kaydet", _btnSaveRect, mx, my);
            }
        }

        private void DrawMenuButton(string text, GuiRect rect, int mx, int my)
        {
            if (mx >= rect.X && mx <= rect.X + rect.W && my >= rect.Y && my <= rect.Y + rect.H)
            {
                Gui.Gui.Canvas.DrawFilledRectangle(_menuHoverPen, rect.X, rect.Y, rect.W, rect.H);
            }
            Gui.Gui.Canvas.DrawString(text, PCScreenFont.Default, _textPen, rect.X + 4, rect.Y + 3);
        }

        private void HandleMouseClick()
        {
            bool isPressedNow = (MouseManager.MouseState == MouseState.Left);

            // Tıklamanın ilk framesini yakalayarak sonsuz döngü donmasını engelliyoruz
            if (isPressedNow && !_wasPressed)
            {
                int mx = (int)MouseManager.X;
                int my = (int)MouseManager.Y;

                // "Dosya" butonuna tıklama kontrolü
                if (mx >= _fileMenuBtnRect.X && mx <= _fileMenuBtnRect.X + _fileMenuBtnRect.W &&
                    my >= _fileMenuBtnRect.Y && my <= _fileMenuBtnRect.Y + _fileMenuBtnRect.H)
                {
                    _isMenuOpen = !_isMenuOpen;
                    _wasPressed = true;
                    return;
                }

                if (_isMenuOpen)
                {
                    if (mx >= _btnNewRect.X && mx <= _btnNewRect.X + _btnNewRect.W && my >= _btnNewRect.Y && my <= _btnNewRect.Y + _btnNewRect.H)
                    {
                        _lines.Clear();
                        _lines.Add("");
                        _currentLineIndex = 0;
                        _fileName = "notlar.txt";
                        _statusMessage = "Yeni temiz dosya acildi.";
                        _isMenuOpen = false;
                    }
                    else if (mx >= _btnOpenRect.X && mx <= _btnOpenRect.X + _btnOpenRect.W && my >= _btnOpenRect.Y && my <= _btnOpenRect.Y + _btnOpenRect.H)
                    {
                        LoadFile();
                        _isMenuOpen = false;
                    }
                    else if (mx >= _btnSaveRect.X && mx <= _btnSaveRect.X + _btnSaveRect.W && my >= _btnSaveRect.Y && my <= _btnSaveRect.Y + _btnSaveRect.H)
                    {
                        SaveFile(); // Donmayan yeni güvenli kayıt motoru tetikleniyor reis
                        _isMenuOpen = false;
                    }
                    else
                    {
                        _isMenuOpen = false;
                    }
                }
            }

            _wasPressed = isPressedNow;
        }

        private void HandleKeyboard()
        {
            if (_isMenuOpen) return; // Menü açıkken editöre harf basılmasın

            if (KeyboardManager.TryReadKey(out KeyEvent keyEvent))
            {
                if (keyEvent.Key == ConsoleKeyEx.Enter)
                {
                    if (_lines.Count < 25)
                    {
                        _lines.Insert(_currentLineIndex + 1, "");
                        _currentLineIndex++;
                    }
                    return;
                }

                if (keyEvent.Key == ConsoleKeyEx.Backspace)
                {
                    if (_lines[_currentLineIndex].Length > 0)
                    {
                        _lines[_currentLineIndex] = _lines[_currentLineIndex].Substring(0, _lines[_currentLineIndex].Length - 1);
                    }
                    else if (_currentLineIndex > 0)
                    {
                        _lines.RemoveAt(_currentLineIndex);
                        _currentLineIndex--;
                    }
                    return;
                }

                if (keyEvent.Key == ConsoleKeyEx.UpArrow)
                {
                    if (_currentLineIndex > 0) _currentLineIndex--;
                    return;
                }
                if (keyEvent.Key == ConsoleKeyEx.DownArrow)
                {
                    if (_currentLineIndex < _lines.Count - 1) _currentLineIndex++;
                    return;
                }

                if (keyEvent.KeyChar >= 32 && keyEvent.KeyChar <= 126)
                {
                    if (_lines[_currentLineIndex].Length < 50)
                    {
                        _lines[_currentLineIndex] += keyEvent.KeyChar;
                    }
                }
            }
        }

        // 🚀 TAMAMEN SENİN TAKTİĞE GÖRE GÜNCELLENEN VE DONMAYAN MOTOR
        private void SaveFile()
        {
            try
            {
                string fullText = string.Join("\n", _lines);
                string path = @"0:\" + _fileName; // Çakışmasız tam kök dizin yolu

                // Eğer eski dosya varsa silip kanalı rahatlatıyoruz reis
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                // Dosyayı oluştur ve ANINDA kapat! Cosmos kilidi bıraksın.
                File.Create(path).Close();

                // Dosya kapalı ve özgür olduğuna göre senin güvenli motor içeriği basabilir
                FileSystem.Write(_fileName, fullText);

                _statusMessage = "LthuOS diske basariyla yazdi!";
            }
            catch (Exception ex)
            {
                _statusMessage = "Hata: " + ex.Message;
            }
        }

        private void LoadFile()
        {
            try
            {
                string path = @"0:\" + _fileName;
                if (File.Exists(path))
                {
                    _lines.Clear();

                    // Okurken de güvenli akış kullanalım reis
                    using (var fs = File.OpenRead(path))
                    {
                        using (var sr = new StreamReader(fs))
                        {
                            string line;
                            while ((line = sr.ReadLine()) != null)
                            {
                                _lines.Add(line);
                            }
                        }
                    }
                    if (_lines.Count == 0) _lines.Add("");
                    _currentLineIndex = 0;
                    _statusMessage = "Dosya basariyla yuklendi.";
                }
                else
                {
                    _statusMessage = "0:\\notlar.txt bulunamadi!";
                }
            }
            catch (Exception ex)
            {
                _statusMessage = "Okuma Hatasi: " + ex.Message;
            }
        }
    }
}