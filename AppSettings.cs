// NOTICE (GPL-2.0): modified in September 2026: optional VPN connect/disconnect task names.
namespace TrayPingMonitor;

public sealed class AppSettings
{
    public string Host { get; set; } = "";               // IP/hostname
    public int IntervalMs { get; set; } = 1000;          // default 1s
    public int LatencyThresholdMs { get; set; } = 150;   // default 150ms
    public bool RunAtStartup { get; set; } = false;

    // Rolling window size is fixed to 20 per requirements.
    public int WindowSize { get; set; } = 20;

    // Optional: names of Task Scheduler tasks run by the tray menu items
    // "Disconnect VPN" / "Connect VPN". Empty = the item is not shown.
    public string? VpnDisconnectTask { get; set; }
    public string? VpnConnectTask { get; set; }
}
