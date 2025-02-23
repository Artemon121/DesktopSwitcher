using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Threading;

namespace DesktopSwitcher
{
    internal static class ExplorerUtils
    {
        private const int WM_KEYDOWN = 0x0100;
        private const int VK_F5 = 0x74;

        [DllImport("Shell32.dll")]
        private static extern int SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        public static void Refresh()
        {
            SHChangeNotify(0x8000000, 0x1000, IntPtr.Zero, IntPtr.Zero);
            
            // Get handle to the Program Manager window (desktop)
            IntPtr hWnd = FindWindow("Progman", null!);

            if (hWnd != IntPtr.Zero)
            {
                PostMessage(hWnd, WM_KEYDOWN, VK_F5, 3);
            }
        }

        public static void Kill()
        {
            foreach (var process in Process.GetProcessesByName("explorer"))
            {
                try
                {
                    process.Kill();
                    process.WaitForExit();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error killing explorer: {ex.Message}");
                }
            }
        }

        public static void TaskKill()
        {
            Process.Start(@"C:\Windows\System32\taskkill.exe", @"/F /IM explorer.exe");
        }

        public static void Restart()
        {
            Kill();
            Thread.Sleep(250);
            if (Process.GetProcessesByName("explorer").Length == 0)
            {
                Process.Start("explorer.exe");
            }
        }

    }
}
