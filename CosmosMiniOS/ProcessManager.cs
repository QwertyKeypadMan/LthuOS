using CosmosMiniOS.Apps;
using System.Collections.Generic;

namespace CosmosMiniOS.System
{
    public static class ProcessManager
    {
        private static List<Process> _list = new List<Process>();

        public static void Start(Process p)
        {
            if (HasProcess(p.Name)) return;
            _list.Add(p);
        }

        public static void Stop(Process p) => _list.Remove(p);

        public static bool HasProcess(string name)
        {
            for (int i = 0; i < _list.Count; i++)
                if (_list[i].Name == name) return true;
            return false;
        }

        public static int Count() => _list.Count;
        public static Process GetAt(int i) => _list[i];

        public static void BringToFront(Process p)
        {
            if (!_list.Contains(p)) return;
            _list.Remove(p);
            _list.Add(p);
        }

        // Sadece logic
        public static void Update()
        {
            for (int i = 0; i < _list.Count; i++)
                try { _list[i]?.Run(); } catch { }
            for (int i = 0; i < ProcessManager.Count(); i++)
            {
                var p = ProcessManager.GetAt(i);
                if (p is FileManager fm)
                {
                    fm.Run(); // Klasör seçim mantığını ve tıkları dinler reis
                }
            }
        }

        // Sadece çizim
        public static void DrawAll()
        {
            for (int i = 0; i < _list.Count; i++)
                try { _list[i]?.Draw(); } catch { }
        }
    }
}