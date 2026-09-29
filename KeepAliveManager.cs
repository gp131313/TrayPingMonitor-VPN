// Added in September 2026 (TrayPingMonitor-VPN): optional auto-restore via a per-user Task Scheduler task.
using System;
using System.Globalization;
using System.Security;

namespace TrayPingMonitor;

/// <summary>
/// Keeps TrayPingMonitor running: a per-user scheduled task that starts this exe at logon and every minute.
/// While the app runs, the task instance stays "running" (MultipleInstances = IgnoreNew), so new triggers are
/// skipped; a second copy started any other way exits at once (single-instance mutex in Program).
/// No admin rights needed. Uses the Task Scheduler COM API (Schedule.Service).
/// </summary>
public static class KeepAliveManager
{
    public const string TaskName = "TrayPingMonitor keepalive";

    private const int TASK_CREATE_OR_UPDATE = 6;
    private const int TASK_LOGON_INTERACTIVE_TOKEN = 3;

    private static dynamic RootFolder()
    {
        var t = Type.GetTypeFromProgID("Schedule.Service")
                ?? throw new InvalidOperationException("Task Scheduler is not available.");
        dynamic svc = Activator.CreateInstance(t)!;
        svc.Connect();
        return svc.GetFolder("\\");
    }

    private static dynamic? GetTask()
    {
        try { return RootFolder().GetTask(TaskName); }
        catch { return null; }
    }

    public static bool IsEnabled()
    {
        try
        {
            var task = GetTask();
            return task is not null && (bool)task.Enabled;
        }
        catch { return false; }
    }

    public static void SetEnabled(bool enabled)
    {
        if (enabled)
        {
            Register();
            return;
        }

        try { RootFolder().DeleteTask(TaskName, 0); }
        catch { /* already absent */ }
    }

    /// <summary>If the task is on but points to another exe (app moved, or an older helper script), re-register it.</summary>
    public static void EnsureUpToDate()
    {
        try
        {
            var task = GetTask();
            if (task is null || !(bool)task.Enabled) return;

            string path = "";
            try { path = ((string)task.Definition.Actions.Item(1).Path).Trim('"'); } catch { }

            if (!string.Equals(path, ExePath(), StringComparison.OrdinalIgnoreCase))
                Register();
        }
        catch { /* best effort */ }
    }

    private static void Register()
    {
        string user = $"{Environment.UserDomainName}\\{Environment.UserName}";
        string start = DateTime.Now.AddMinutes(1).ToString("yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture);

        string xml = $@"<?xml version=""1.0"" encoding=""UTF-16""?>
<Task version=""1.2"" xmlns=""http://schemas.microsoft.com/windows/2004/02/mit/task"">
  <RegistrationInfo>
    <Description>Restarts TrayPingMonitor if it is not running (menu: Auto-restore if closed).</Description>
  </RegistrationInfo>
  <Triggers>
    <LogonTrigger>
      <Enabled>true</Enabled>
      <UserId>{SecurityElement.Escape(user)}</UserId>
      <Delay>PT30S</Delay>
    </LogonTrigger>
    <TimeTrigger>
      <Repetition>
        <Interval>PT1M</Interval>
        <StopAtDurationEnd>false</StopAtDurationEnd>
      </Repetition>
      <StartBoundary>{start}</StartBoundary>
      <Enabled>true</Enabled>
    </TimeTrigger>
  </Triggers>
  <Principals>
    <Principal id=""Author"">
      <UserId>{SecurityElement.Escape(user)}</UserId>
      <LogonType>InteractiveToken</LogonType>
      <RunLevel>LeastPrivilege</RunLevel>
    </Principal>
  </Principals>
  <Settings>
    <MultipleInstancesPolicy>IgnoreNew</MultipleInstancesPolicy>
    <DisallowStartIfOnBatteries>false</DisallowStartIfOnBatteries>
    <StopIfGoingOnBatteries>false</StopIfGoingOnBatteries>
    <StartWhenAvailable>true</StartWhenAvailable>
    <ExecutionTimeLimit>PT0S</ExecutionTimeLimit>
    <AllowHardTerminate>true</AllowHardTerminate>
    <Enabled>true</Enabled>
    <Priority>7</Priority>
  </Settings>
  <Actions Context=""Author"">
    <Exec>
      <Command>""{SecurityElement.Escape(ExePath())}""</Command>
    </Exec>
  </Actions>
</Task>";

        // userId / password / sddl are optional VARIANTs: Type.Missing = "not specified".
        RootFolder().RegisterTask(TaskName, xml, TASK_CREATE_OR_UPDATE, Type.Missing, Type.Missing,
            TASK_LOGON_INTERACTIVE_TOKEN, Type.Missing);
    }

    private static string ExePath() => Environment.ProcessPath ?? "TrayPingMonitor.exe";
}
