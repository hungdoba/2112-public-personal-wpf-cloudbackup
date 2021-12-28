using System;
using System.Runtime.InteropServices;

namespace AutoBackUpToCloud
{
    public class User32API
    {

        public const int HWND_BROADCAST = 0xffff;

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int RegisterWindowMessage(string lpString);

        public static readonly int WM_SHOWME = RegisterWindowMessage("WM_SHOWME");

        [DllImport("user32")]
        public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);

    }
}
