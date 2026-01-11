using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DevKit.Debuggers.Utils;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;

namespace DevKit.Debuggers.Windows;

public class AgentDebugger(Agent agent) : DebuggerWindow
{
    public override string Name => $"DevKit | Agent | {agent.Name}";

    private bool _stateIsOpen;
    private bool _flagsIsOpen;

    protected override void Render()
    {
        if (Mission.Current == null || Mission.Current.IsFinalized)
        {
            Imgui.Text("No active mission.");
            return;
        }

        if (!agent.IsActive())
        {
            Imgui.Text("Missing agent.");
            return;
        }

        Imgui.Text("Agent: " + agent.Name);
        if (agent.IsHero)
        {
            var hero = (agent.Character as CharacterObject)?.HeroObject;
            if (hero != null)
            {
                Imgui.SameLine(0, 10);
                SmallButton(
                    "Hero",
                    () => Campaign.Current.EncyclopediaManager.GoToLink(hero.EncyclopediaLink),
                    YellowStyleColor
                );
            }
        }
        Imgui.Text(
            $"Health: {agent.Health}/{agent.HealthLimit} ({(int)(agent.Health / agent.HealthLimit * 100f)}%%)"
        );
        _stateIsOpen = DropdownButton(
            "State: " + agent.State,
            _stateIsOpen,
            () =>
            {
                foreach (var mode in Enum.GetValues(typeof(AgentState)))
                {
                    Button(mode.ToString(), () => agent.State = (AgentState)mode);
                    Imgui.SameLine(0, 10);
                }

                Imgui.NewLine();
                Imgui.Separator();
            }
        );
        // TODO: Improve. Move to collapse, list as checkboxes
        _flagsIsOpen = DropdownButton(
            "Flags",
            _flagsIsOpen,
            () =>
            {
                var flags = agent.GetAgentFlags().ToString().Split([", "], StringSplitOptions.None);
                for (var i = 0; i < flags.Length; i += 4)
                {
                    var lineFlags = flags.Skip(i).Take(4);
                    Text(string.Join(", ", lineFlags));
                }
                Imgui.Separator();
            }
        );
        Imgui.Text(agent.Team?.ToString() ?? "No Team");

        Imgui.NewLine();
        RenderAgentUtilities();
        Imgui.NewLine();

        Collapse("Agent Driven Properties", RenderAgentDrivenProperties);
        Collapse("Action", RenderAgentActions);
        Collapse("Components", RenderAgentComponents);

        if (agent.IsAIControlled)
            Collapse("AI", RenderAiInfo);

        // Other ideas:
        // Equipment
        // EventControlFlags
        // Formation
        // monster
        // agent scripted stuff
    }

    private void RenderAgentUtilities()
    {
        Button("Highlight", agent.HighlightAgent);
        Imgui.SameLine(0, 10);
        Button("Toggle invulnerable", agent.ToggleInvulnerable);
        Imgui.SameLine(0, 10);
        Button("Break", Break, "Break into the debugger (if attached)");
        Button(
            "Teleport to agent",
            () => Mission.Current.MainAgent?.TeleportToPosition(agent.Position)
        );
        Imgui.SameLine(0, 10);
        Button(
            "Teleport to player",
            () => agent.TeleportToPosition(Mission.Current.MainAgent.Position)
        );
        // More ideas: drop items, kill
    }

    private void RenderAgentDrivenProperties()
    {
        var properties = agent.AgentDrivenProperties;
        foreach (var prop in properties.GetType().GetProperties())
        {
            Imgui.Text($"- {prop.Name}: {prop.GetValue(properties)}");

            if (prop.Name.Contains("Ai") || prop.Name.Contains("AI"))
            {
                Imgui.SameLine(0, 10);
                Text("(Ai)", PurpleStyleColor);
            }
        }
    }

    private void RenderAgentActions()
    {
        Imgui.Columns(2);
        RenderActionInfo(0);
        Imgui.NextColumn();
        RenderActionInfo(1);
        Imgui.Columns(1);
    }

    private void RenderAgentComponents()
    {
        var toRemove = new List<AgentComponent>();
        foreach (var component in agent.Components)
        {
            var name = component.GetType().Name;

            Imgui.Text($"- {name}");
            Imgui.SameLine(0, 10);
            SmallButton($"Remove##{name}", () => toRemove.Add(component));
        }

        foreach (var comp in toRemove)
            agent.RemoveComponent(comp);
    }

    private void RenderAiInfo()
    {
        // agent.SetAIBehaviorParams();
        // agent.SetAIBehaviorValues();
        // agent.GetDefendMovementFlag();  // Where its supposed to auto block (?)
        // agent.GetMovementDirection();
        // agent.InvalidateTargetAgent();
        // agent.SetLookAgent();

        Button(agent.IsPaused ? "Unpause" : "Pause", () => agent.SetIsAIPaused(!agent.IsPaused));
        Text("State: " + agent.AIStateFlags);
    }

    private void RenderActionInfo(int channelId)
    {
        Imgui.Text($"Channel: {channelId}");
        Imgui.NewLine();

        Imgui.Text($"Action: {agent.GetCurrentAction(channelId).GetName()}");
        Imgui.Text($"Priority: {agent.GetCurrentActionPriority(channelId)}");
        Imgui.Text($"Type: {agent.GetCurrentActionType(channelId)}");
        Imgui.Text($"Stage: {agent.GetCurrentActionStage(channelId)}");
        Imgui.Text($"Weight: {agent.GetActionChannelWeight(channelId):F3}");
        Imgui.Text($"Current Weight: {agent.GetActionChannelCurrentActionWeight(channelId):F3}");
        Imgui.Text($"Progress: {agent.GetCurrentActionProgress(channelId):F2}");
    }

    private void Break()
    {
        Debugger.Break();
    }
}
