// NOTICE (GPL-2.0): modified in September 2026: single-instance guard and --keepalive on|off|status switch.
using System;
using System.Threading;
using System.Windows.Forms;

namespace TrayPingMonitor;

internal static class Program
{
    [STAThread]
    static int Main(string[] args)
    {
        // Command line: TrayPingMonitor.exe --keepalive on|off|status  (exit code 0 = on / done, 1 = off, 2 = error)
        if (args.Length == 2 && args[0].Equals("--keepalive", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                switch (args[1].ToLowerInvariant())
                {
                    case "on": KeepAliveManager.SetEnabled(true); return KeepAliveManager.IsEnabled() ? 0 : 2;
                    case "off": KeepAliveManager.SetEnabled(false); return KeepAliveManager.IsEnabled() ? 2 : 0;
                    case "status": return KeepAliveManager.IsEnabled() ? 0 : 1;
                    default: return 2;
                }
            }
            catch { return 2; }
        }

        // One tray icon per user session: the keepalive task and the Run key may both start the app.
        using var mutex = new Mutex(true, @"Local\TrayPingMonitor-" + Environment.UserName, out bool createdNew);
        if (!createdNew) return 0;

        ApplicationConfiguration.Initialize();
        Application.Run(new MainApplicationContext());
        return 0;
    }
}
