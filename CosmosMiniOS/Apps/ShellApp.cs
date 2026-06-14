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
    public class GuiShell : Process
    {
        private List<string> _history = new List<string>();
        private string _currentInput = "";
        private string _currentDir = @"0:\";

        // Grafik Renk Kalemleri
        private static readonly Pen _textPen = new Pen(Color.LimeGreen);
        private static readonly Pen _inputPen = new Pen(Color.White);
        private static readonly Pen _promptPen = new Pen(Color.Cyan);

        private const int LineHeight = 18;

        public GuiShell()
        {
            _history.Add("LthuOS GUI Terminal.");
            _history.Add("If you want to see the commands, type help.");
            _history.Add("");
        }

        public override void Run()
        {
            HandleKeyboard();
        }

        public override void Draw()
        {
            Window.DrawTop(this, Gui.Gui.Canvas);
            Window.DrawBody(WindowData.WinPos, Color.FromArgb(255, 15, 15, 20), Gui.Gui.Canvas);

            int wx = WindowData.WinPos.X;
            int wy = WindowData.WinPos.Y;
            int ww = WindowData.WinPos.W;
            int wh = WindowData.WinPos.H;

            int startY = wy + Window.TopSize + 10;
            int itemX = wx + 12;

 
            int maxLines = (wh - Window.TopSize - 35) / LineHeight;
            int startLine = Math.Max(0, _history.Count - maxLines);

            int currentY = startY;
            for (int i = startLine; i < _history.Count; i++)
            {
                Gui.Gui.Canvas.DrawString(_history[i], PCScreenFont.Default, _textPen, itemX, currentY);
                currentY += LineHeight;
            }

            string prompt = _currentDir + "> ";
            Gui.Gui.Canvas.DrawString(prompt, PCScreenFont.Default, _promptPen, itemX, currentY);

            int promptWidth = prompt.Length * 8;
            Gui.Gui.Canvas.DrawString(_currentInput + "_", PCScreenFont.Default, _inputPen, itemX + promptWidth, currentY);
        }

        private void HandleKeyboard()
        {
            if (KeyboardManager.TryReadKey(out KeyEvent keyEvent))
            {
                if (keyEvent.Key == ConsoleKeyEx.Enter)
                {
                    string input = _currentInput.Trim();
                    _history.Add(_currentDir + "> " + _currentInput); 
                    _currentInput = ""; 

                    if (!string.IsNullOrEmpty(input))
                    {
                        ExecuteGuiCommand(input);
                    }
                }
                else if (keyEvent.Key == ConsoleKeyEx.Backspace)
                {
                    if (_currentInput.Length > 0)
                    {
                        _currentInput = _currentInput.Substring(0, _currentInput.Length - 1);
                    }
                }
                else
                {
                    if (keyEvent.KeyChar >= 32 && keyEvent.KeyChar <= 126)
                    {
                        if (_currentInput.Length < 45)
                        {
                            _currentInput += keyEvent.KeyChar;
                        }
                    }
                }
            }
        }


        private void ExecuteGuiCommand(string input)
        {
            input = input.Trim();
            string command = input;
            string args = "";
            int spaceIndex = input.IndexOf(' ');

            if (spaceIndex >= 0)
            {
                command = ToLowerAscii(input.Substring(0, spaceIndex));
                args = input.Substring(spaceIndex + 1).Trim();
            }
            else
            {
                command = ToLowerAscii(input);
            }


            if (command == "help")
            {
                _history.Add("Komutlar:");
                _history.Add("  help, ver, cls, echo");
                _history.Add("  dir / ls, touch, del");
                _history.Add("  reboot, shutdown");
            }
            else if (command == "about" || command == "ver")
            {
                _history.Add("LthuOS 1.1 [GUI Mode]");
                _history.Add("Aparat sistemi devrede.");
            }
            else if (command == "clear" || command == "cls")
            {
                _history.Clear();
            }
            else if (command == "echo")
            {
                _history.Add(args);
            }
            else if (command == "ls" || command == "dir")
            {
                try
                {
                    foreach (var dir in Directory.GetDirectories(_currentDir))
                        _history.Add("<DIR> " + Path.GetFileName(dir));
                    foreach (var file in Directory.GetFiles(_currentDir))
                        _history.Add("      " + Path.GetFileName(file));
                }
                catch
                {
                    _history.Add("Hata: Dizin okunamadi.");
                }
            }
            else if (command == "touch")
            {
                if (string.IsNullOrEmpty(args)) { _history.Add("Kullanim: touch dosya.txt"); return; }
                try
                {
                    string path = Path.Combine(_currentDir, args);
                    if (!File.Exists(path))
                    {
                        File.Create(path).Close();
                        _history.Add("Dosya olusturuldu: " + args);
                    }
                    else { _history.Add("Dosya zaten var!"); }
                }
                catch (Exception ex) { _history.Add("Hata: " + ex.Message); }
            }
            else if (command == "del")
            {
                if (string.IsNullOrEmpty(args)) { _history.Add("Kullanim: del dosya.txt"); return; }
                try
                {
                    string path = Path.Combine(_currentDir, args);
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                        _history.Add("Dosya silindi: " + args);
                    }
                    else { _history.Add("Dosya bulunamadi."); }
                }
                catch (Exception ex) { _history.Add("Hata: " + ex.Message); }
            }
            else if (command == "reboot")
            {
                Cosmos.System.Power.Reboot();
            }
            else if (command == "shutdown")
            {
                Cosmos.System.Power.Shutdown();
            }
            else
            {
                _history.Add("Bilinmeyen komut: " + command);
            }

            _history.Add(""); 
        }

        private string ToLowerAscii(string text)
        {
            string result = "";
            for (int i = 0; i < text.Length; i++)
                result += ToLowerChar(text[i]);
            return result;
        }

        private char ToLowerChar(char c)
            => (c >= 'A' && c <= 'Z') ? (char)(c + 32) : c;
    }
}