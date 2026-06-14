using CosmosMiniOS.System;
using System;
using Sys = Cosmos.System;

namespace CosmosMiniOS
{
    public sealed class Shell
    {
        private readonly OpRuntime _opRuntime;

        public Shell()
        {
            _opRuntime = new OpRuntime();
        }

        public void Execute(string input)
        {
            ExecuteInternal(input, 0);
        }

        private void ExecuteInternal(string input, int depth)
        {
            if (input == null) return;
            input = input.Trim();
            if (input == "") return;

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

            if (command == "help") ShowHelp();
            else if (command == "about" || command == "ver") ShowAbout();
            else if (command == "clear" || command == "cls") Console.Clear();
            else if (command == "echo") Console.WriteLine(args);
            else if (command == "pause")
            {
                Console.WriteLine("Devam etmek icin Enter'a bas.");
                Console.ReadLine();
            }
            else if (command == "mem") FileSystem.PrintInfo();
            else if (command == "calc") Calculate(args);
            else if (command == "ls" || command == "dir") FileSystem.PrintList();
            else if (command == "touch") FileSystem.Touch(args);
            else if (command == "write") WriteFile(args);
            else if (command == "append") AppendFile(args);
            else if (command == "cat" || command == "type") ReadFile(args);
            else if (command == "copy") CopyFile(args);
            else if (command == "ren" || command == "rename") RenameFile(args);
            else if (command == "del") FileSystem.Delete(args);
            else if (command == "fsinfo") FileSystem.PrintInfo();
            else if (command == "apps") _opRuntime.ListApps();
            else if (command == "run") RunTarget(args, depth);
            else if (command == "opengui") OpenGui();
            else if (command == "reboot") Sys.Power.Reboot();
            else if (command == "shutdown") Sys.Power.Shutdown();
            else Console.WriteLine("Bilinmeyen komut: " + command);
        }

        private static void OpenGui()
        {
            Gui.Gui.StartGUI();
            Kernel.RunGui = true;
        }

        private void RunTarget(string args, int depth)
        {
            if (EndsWithAscii(args, ".op")) { _opRuntime.Run(args); return; }
            if (EndsWithAscii(args, ".ops")) { RunScript(args, depth); return; }
            Console.WriteLine("Kullanim: run hello.op veya run start.ops");
        }

        private void RunScript(string fileName, int depth)
        {
            if (depth >= 4) { Console.WriteLine("Script ic ice calistirma siniri asildi."); return; }

            string content;
            if (!FileSystem.TryRead(fileName, out content))
            {
                Console.WriteLine("Script dosyasi bulunamadi.");
                return;
            }

            int start = 0, index = 0;
            while (index <= content.Length)
            {
                if (index == content.Length || content[index] == ';')
                {
                    string line = content.Substring(start, index - start).Trim();
                    if (line != "") ExecuteInternal(line, depth + 1);
                    start = index + 1;
                }
                index++;
            }
        }

        private static void ReadFile(string name)
        {
            string content;
            if (FileSystem.TryRead(name, out content))
                Console.WriteLine(content);
            else
                Console.WriteLine("Dosya bulunamadi: " + name);
        }

        private static void WriteFile(string args)
        {
            string name, text;
            if (!ReadTwoParts(args, out name, out text)) { Console.WriteLine("Kullanim: write not.txt merhaba"); return; }
            FileSystem.Write(name, text);
        }

        private static void AppendFile(string args)
        {
            string name, text;
            if (!ReadTwoParts(args, out name, out text)) { Console.WriteLine("Kullanim: append not.txt merhaba"); return; }
            FileSystem.Append(name, text);
        }

        private static void CopyFile(string args)
        {
            string source, target;
            if (!ReadTwoParts(args, out source, out target)) { Console.WriteLine("Kullanim: copy a.txt b.txt"); return; }
            FileSystem.Copy(source, target);
        }

        private static void RenameFile(string args)
        {
            string source, target;
            if (!ReadTwoParts(args, out source, out target)) { Console.WriteLine("Kullanim: ren eski.txt yeni.txt"); return; }
            FileSystem.Rename(source, target);
        }

        private static void ShowHelp()
        {
            Console.WriteLine("Komutlar:");
            Console.WriteLine("  help, ver, cls, echo, pause");
            Console.WriteLine("  dir/ls, type/cat, copy, ren, del");
            Console.WriteLine("  touch, write, append");
            Console.WriteLine("  mem, fsinfo, calc");
            Console.WriteLine("  apps, run <app.op>, run <file.ops>");
            Console.WriteLine("  opengui, reboot, shutdown");
        }

        private static void ShowAbout()
        {
            Console.WriteLine("LthuOS 1.1");
            Console.WriteLine("Kernel: Cosmos.System.Kernel");
            Console.WriteLine("Dosya sistemi: CosmosVFS");
        }

        private static void Calculate(string args)
        {
            string leftText, operation, rightText;
            if (!ReadCalcParts(args, out leftText, out operation, out rightText))
            {
                Console.WriteLine("Kullanim: calc 10 + 5");
                return;
            }

            int left, right;
            if (!TryParseInt(leftText, out left) || !TryParseInt(rightText, out right))
            {
                Console.WriteLine("Sadece tam sayilar destekleniyor.");
                return;
            }

            if (operation == "+") Console.WriteLine(left + right);
            else if (operation == "-") Console.WriteLine(left - right);
            else if (operation == "*") Console.WriteLine(left * right);
            else if (operation == "/")
            {
                if (right == 0) { Console.WriteLine("Sifira bolme yapilamaz."); return; }
                Console.WriteLine(left / right);
            }
            else Console.WriteLine("Desteklenen islemler: + - * /");
        }

        private static bool ReadTwoParts(string args, out string first, out string rest)
        {
            first = ""; rest = "";
            if (args == null) return false;
            args = args.Trim();
            int sp = args.IndexOf(' ');
            if (sp < 0) return false;
            first = args.Substring(0, sp).Trim();
            rest = args.Substring(sp + 1).Trim();
            return first != "" && rest != "";
        }

        private static bool ReadCalcParts(string args, out string left, out string operation, out string right)
        {
            left = ""; operation = ""; right = "";
            args = args.Trim();
            int f = args.IndexOf(' ');
            if (f < 0) return false;
            left = args.Substring(0, f).Trim();
            string rest = args.Substring(f + 1).Trim();
            int s = rest.IndexOf(' ');
            if (s < 0) return false;
            operation = rest.Substring(0, s).Trim();
            right = rest.Substring(s + 1).Trim();
            return left != "" && operation != "" && right != "";
        }

        private static bool TryParseInt(string text, out int value)
        {
            value = 0;
            if (text == null || text == "") return false;
            int sign = 1, index = 0;
            if (text[0] == '-') { sign = -1; index = 1; }
            if (index >= text.Length) return false;
            while (index < text.Length)
            {
                char c = text[index];
                if (c < '0' || c > '9') return false;
                value = value * 10 + (c - '0');
                index++;
            }
            value *= sign;
            return true;
        }

        private static bool EndsWithAscii(string text, string suffix)
        {
            if (text == null || suffix == null || text.Length < suffix.Length) return false;
            int offset = text.Length - suffix.Length, index = 0;
            while (index < suffix.Length)
            {
                if (ToLowerChar(text[offset + index]) != suffix[index]) return false;
                index++;
            }
            return true;
        }

        private static string ToLowerAscii(string text)
        {
            string result = "";
            for (int i = 0; i < text.Length; i++)
                result += ToLowerChar(text[i]);
            return result;
        }

        private static char ToLowerChar(char c)
            => (c >= 'A' && c <= 'Z') ? (char)(c + 32) : c;
    }
}