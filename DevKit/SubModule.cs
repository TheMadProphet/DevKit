using DevKit.Configuration;
using DevKit.Debuggers;
using DevKit.Debuggers.Windows;
using DevKit.Hotkeys;
using HarmonyLib;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;

namespace DevKit;

public class SubModule : MBSubModuleBase
{
    private static Harmony HarmonyInstance { get; set; }

    private GameKey _openControlPanelKey;
    private GameKey _openMobilePartyDebuggerKey;
    private GameKey _openCampaignEventsDebuggerKey;
    private GameKey _openMissionDebuggerKey;
    private GameKey _openAgentSelectorKey;

    protected override void OnSubModuleLoad()
    {
        DevKitConfigIO.Load();

        HarmonyInstance = new Harmony("mod.harmony.devkit");
        HarmonyInstance.PatchAll();

        DevKitHotKeyManager.Initialize();
        var devkitCategory = HotKeyManager.GetCategory(nameof(DevKitGameKeyContext));

        _openControlPanelKey = devkitCategory.GetGameKey(
            (int)DevKitGameKeyContext.KeyMap.OpenControlPanel
        );
        _openMobilePartyDebuggerKey = devkitCategory.GetGameKey(
            (int)DevKitGameKeyContext.KeyMap.OpenMobilePartyDebugger
        );
        _openCampaignEventsDebuggerKey = devkitCategory.GetGameKey(
            (int)DevKitGameKeyContext.KeyMap.OpenCampaignEventsDebugger
        );
        _openMissionDebuggerKey = devkitCategory.GetGameKey(
            (int)DevKitGameKeyContext.KeyMap.OpenMissionDebugger
        );
        _openAgentSelectorKey = devkitCategory.GetGameKey(
            (int)DevKitGameKeyContext.KeyMap.OpenAgentSelector
        );
    }

    protected override void OnApplicationTick(float dt)
    {
        var isShiftDown = Input.IsKeyDown(InputKey.LeftShift);
        if (Input.IsKeyPressed(_openControlPanelKey.KeyboardKey.InputKey))
            WindowManager.ControlPanel.Toggle();

        if (Input.IsKeyPressed(_openMobilePartyDebuggerKey.KeyboardKey.InputKey))
        {
            if (isShiftDown)
                WindowManager.AddWindow(new MobilePartyDebugger());
            else
                WindowManager.MobilePartyDebugger.Toggle();
        }

        if (Input.IsKeyPressed(_openCampaignEventsDebuggerKey.KeyboardKey.InputKey))
        {
            if (isShiftDown)
                WindowManager.AddWindow(new CampaignEventsDebugger());
            else
                WindowManager.CampaignEventsDebugger.Toggle();
        }

        if (Input.IsKeyPressed(_openMissionDebuggerKey.KeyboardKey.InputKey))
        {
            if (isShiftDown)
                WindowManager.AddWindow(new MissionDebugger());
            else
                WindowManager.MissionDebugger.Toggle();
        }

        if (Input.IsKeyPressed(_openAgentSelectorKey.KeyboardKey.InputKey))
        {
            if (isShiftDown)
                WindowManager.AddWindow(new AgentSelector());
            else
                WindowManager.AgentSelector.Toggle();
        }
    }

    public override void OnMissionBehaviorInitialize(Mission mission)
    {
        mission.AddMissionBehavior(new DebuggerWindowMissionLogic());
    }
}
