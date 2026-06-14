using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;
using System;
using System.IO;

namespace CosmosMiniOS.System
{
    public static class FileSystem
    {
        private static CosmosVFS _vfs;
        private static bool _ready = false;
        private static string _root = @"0:\";

        public static bool Ready => _ready;

        public static void Init()
        {
            try
            {
                _vfs = new CosmosVFS();
                VFSManager.RegisterVFS(_vfs);
                _ready = true;
            }
            catch
            {
                _ready = false;
            }
        }

        public static bool Exists(string name)
        {
            if (!_ready) return false;
            try { return File.Exists(_root + name); }
            catch { return false; }
        }

        public static void Touch(string name)
        {
            if (!_ready) return;
            try
            {
                string path = _root + name;
                if (!File.Exists(path))
                    File.WriteAllText(path, "");
            }
            catch { }
        }

        public static void Write(string name, string content)
        {
            if (!_ready) return;
            try { File.WriteAllText(_root + name, content); }
            catch { }
        }

        public static void Append(string name, string content)
        {
            if (!_ready) return;
            try
            {
                string path = _root + name;
                string existing = "";
                if (File.Exists(path))
                    existing = File.ReadAllText(path);
                File.WriteAllText(path, existing + content);
            }
            catch { }
        }

        public static bool TryRead(string name, out string content)
        {
            content = "";
            if (!_ready) return false;
            try
            {
                string path = _root + name;
                if (!File.Exists(path)) return false;
                content = File.ReadAllText(path);
                return true;
            }
            catch { return false; }
        }

        public static void Delete(string name)
        {
            if (!_ready) return;
            try { File.Delete(_root + name); }
            catch { }
        }

        public static void Copy(string source, string target)
        {
            if (!_ready) return;
            try
            {
                // File.Copy yerine manuel oku-yaz (IL2CPU uyumu)
                string content = File.ReadAllText(_root + source);
                File.WriteAllText(_root + target, content);
            }
            catch { }
        }

        // File.Move IL2CPU'da desteklenmiyor — kopyala + sil
        public static void Rename(string oldName, string newName)
        {
            if (!_ready) return;
            try
            {
                string src = _root + oldName;
                string dst = _root + newName;
                if (!File.Exists(src)) return;
                string content = File.ReadAllText(src);
                File.WriteAllText(dst, content);
                File.Delete(src);
            }
            catch { }
        }

        public static string[] List()
        {
            if (!_ready) return new string[0];
            try
            {
                string[] files = Directory.GetFiles(_root);
                for (int i = 0; i < files.Length; i++)
                    files[i] = Path.GetFileName(files[i]);
                return files;
            }
            catch { return new string[0]; }
        }

        public static int Count()
        {
            return List().Length;
        }

        public static void PrintList()
        {
            var files = List();
            if (files.Length == 0) { Console.WriteLine("Disk bos."); return; }
            for (int i = 0; i < files.Length; i++)
                Console.WriteLine("  " + files[i]);
        }

        public static void PrintInfo()
        {
            if (!_ready) { Console.WriteLine("Dosya sistemi hazir degil."); return; }
            try
            {
                var drive = new DriveInfo("0");
                Console.WriteLine("Disk: " + drive.Name);
                Console.WriteLine("Toplam: " + drive.TotalSize / 1024 + " KB");
                Console.WriteLine("Bos: " + drive.AvailableFreeSpace / 1024 + " KB");
            }
            catch { Console.WriteLine("Disk bilgisi alinamadi."); }
        }
    }
}