using System.Collections.Generic;
using DevKit.Debuggers.Windows;
using HarmonyLib;
using TaleWorlds.ScreenSystem;

namespace DevKit.Debuggers;

[HarmonyPatch]
public static class WindowManager
{
    private static int _nextId = 1;
    public static int NextId => _nextId++;

    public static readonly DebuggerWindow ControlPanel = new ControlPanel();
    public static readonly DebuggerWindow MissionDebugger = new MissionDebugger();
    public static readonly DebuggerWindow CampaignEventsDebugger = new CampaignEventsDebugger();
    public static readonly DebuggerWindow MobilePartyDebugger = new MobilePartyDebugger();
    public static readonly DebuggerWindow AgentSelector = new AgentSelector();
    private static readonly List<DebuggerWindow> Windows =
    [
        ControlPanel,
        MissionDebugger,
        CampaignEventsDebugger,
        MobilePartyDebugger,
        AgentSelector
    ];
    private static readonly List<DebuggerWindow> WindowsToAdd = [];
    private static readonly List<DebuggerWindow> WindowsToRemove = [];

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ScreenManager), "LateTick")]
    public static void Tick()
    {
        foreach (var window in Windows)
            window.Tick();

        if (WindowsToAdd.Count > 0)
        {
            Windows.AddRange(WindowsToAdd);
            WindowsToAdd.Clear();
        }

        if (WindowsToRemove.Count > 0)
        {
            foreach (var window in WindowsToRemove)
            {
                window.Dispose();
                Windows.Remove(window);
            }
            WindowsToRemove.Clear();
        }
    }

    public static void AddWindow(DebuggerWindow window, bool open = true)
    {
        // Defer addition to avoid modifying Windows list during Tick() iteration
        // (e.g., when called from UI button clicks during window.Tick())
        WindowsToAdd.Add(window);

        if (open)
            window.Toggle();
    }

    public static void RemoveWindow(DebuggerWindow window)
    {
        // Defer removal to avoid modifying Windows list during Tick() iteration
        WindowsToRemove.Add(window);
    }

    public static IEnumerable<T> GetAllWindows<T>()
        where T : DebuggerWindow
    {
        foreach (var window in Windows)
        {
            if (window is T typedWindow)
                yield return typedWindow;
        }
    }
}
