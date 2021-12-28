using System;
using System.Diagnostics;

namespace AutoBackUpToCloud
{
    public sealed class SingleInstance
    {
        public static bool AlreadyRunning()
        {
            bool running = false;
            try
            {
                // Getting collection of process  
                Process currentProcess = Process.GetCurrentProcess();

                // Check with other process already running   
                foreach (var p in Process.GetProcesses())
                {
                    if (p.Id != currentProcess.Id) // Check running process   
                    {
                        if (p.ProcessName.Equals(currentProcess.ProcessName) == true)
                        {
                            running = true;

                            User32API.PostMessage(
                                            (IntPtr)User32API.HWND_BROADCAST,
                                            User32API.WM_SHOWME,
                                            IntPtr.Zero,
                                            IntPtr.Zero);

                            break;
                        }
                    }
                }
            }
            catch { }
            return running;
        }
    }
}
