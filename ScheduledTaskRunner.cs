// Added in September 2026 (TrayPingMonitor-VPN): run a named Task Scheduler task on demand.
using System;

namespace TrayPingMonitor;

/// <summary>
/// Starts an existing scheduled task by name (Task Scheduler COM API). Used by the tray menu items
/// "Disconnect VPN" / "Connect VPN": the tasks themselves (usually with highest privileges) do the work,
/// so the tray app stays non-elevated. A user can start tasks registered for their own account.
/// </summary>
public static class ScheduledTaskRunner
{
    public static void Run(string taskName)
    {
        var t = Type.GetTypeFromProgID("Schedule.Service")
                ?? throw new InvalidOperationException("Task Scheduler is not available.");
        dynamic svc = Activator.CreateInstance(t)!;
        svc.Connect();
        dynamic folder = svc.GetFolder("\\");

        dynamic task;
        try { task = folder.GetTask(taskName); }
        catch { throw new InvalidOperationException($"Scheduled task \"{taskName}\" not found."); }

        // IRegisteredTask::Run takes a VARIANT that must be VT_EMPTY or VT_NULL when there are no parameters.
        // Type.Missing is marshalled as VT_ERROR (DISP_E_PARAMNOTFOUND) and fails with E_INVALIDARG
        // ("Value does not fall within the expected range"); DBNull.Value is marshalled as VT_NULL.
        task.Run(DBNull.Value);
    }
}
