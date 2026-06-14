using CosmosMiniOS.System;
using System;

namespace CosmosMiniOS
{
    public class OpRuntime
    {
        public OpRuntime() { }

        // Uygulama listesi
        public void ListApps()
        {
            Console.WriteLine("Uygulamalar:");
            var files = FileSystem.List();
            bool found = false;
            for (int i = 0; i < files.Length; i++)
            {
                if (EndsWithAscii(files[i], ".op"))
                {
                    Console.WriteLine("  " + files[i]);
                    found = true;
                }
            }
            if (!found) Console.WriteLine("  Hic .op dosyasi bulunamadi.");
        }

        // .op dosyasýný çalýþtýr
        public void Run(string fileName)
        {
            string content;
            if (!FileSystem.TryRead(fileName, out content))
            {
                Console.WriteLine("Dosya bulunamadi: " + fileName);
                return;
            }

            // Satýr satýr çalýþtýr (; ile ayrýlmýþ)
            int start = 0, index = 0;
            while (index <= content.Length)
            {
                if (index == content.Length || content[index] == ';')
                {
                    string line = content.Substring(start, index - start).Trim();
                    if (line != "")
                        ExecuteLine(line);
                    start = index + 1;
                }
                index++;
            }
        }

        private void ExecuteLine(string line)
        {
            if (line.StartsWith("print "))
                Console.WriteLine(line.Substring(6));
            else if (line == "cls" || line == "clear")
                Console.Clear();
            else
                Console.WriteLine("[op] Bilinmeyen komut: " + line);
        }

        private static bool EndsWithAscii(string text, string suffix)
        {
            if (text == null || suffix == null || text.Length < suffix.Length) return false;
            int offset = text.Length - suffix.Length;
            for (int i = 0; i < suffix.Length; i++)
                if (ToLower(text[offset + i]) != suffix[i]) return false;
            return true;
        }

        private static char ToLower(char c)
            => (c >= 'A' && c <= 'Z') ? (char)(c + 32) : c;
    }
}