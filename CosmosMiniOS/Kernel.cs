using Cosmos.Core.Memory;
using CosmosMiniOS.System;
using System;
using Sys = Cosmos.System;

namespace CosmosMiniOS
{
    public class Kernel : Sys.Kernel
    {
        private Shell _shell;
        public static bool RunGui = false;
        private int _heapTimer = 0;

        protected override void BeforeRun()
        {
            FileSystem.Init();
            _shell = new Shell();

            Console.Clear();
            Console.WriteLine("LthuOS has booted.");
            Console.WriteLine(FileSystem.Ready ? "FileSystem is ready." : "Dosya sistemi baslatýlamadý.");
            Console.WriteLine("To see the commands, type help.");
            Console.WriteLine();
        }

        protected override void Run()
        {
            if (RunGui)
            {
                Gui.Gui.Update();
            }
            else
            {
                Console.Write("lthuos> ");
                string input = Console.ReadLine();
                _shell.Execute(input);
            }

            _heapTimer++;
            if (_heapTimer >= 1)
            {
                Heap.Collect();
                _heapTimer = 0;
            }
        }
    }
}