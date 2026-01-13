using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using DevKit.Debuggers.Utils;
using HarmonyLib;
using SandBox.View.Map;
using SandBox.View.Map.Visuals;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace DevKit.Debuggers.Windows;

public class MobilePartyDebugger : DebuggerWindow
{
    public override string Name => "DevKit | Mobile Party Debugger";

    private bool _firstTime = true;
    private MobileParty _mobileParty;

    private bool _showIdButtons = true;
    private bool _showEncyclopediaButtons = true;
    private bool _autoSelectOnHover = false;

    protected override void Render()
    {
        if (Campaign.Current == null || Campaign.Current.MapSceneWrapper == null)
        {
            _firstTime = true;
            Imgui.Text("No active campaign.");
            return;
        }

        if (_firstTime && MobileParty.MainParty != null)
        {
            _firstTime = false;
            _mobileParty = MobileParty.MainParty;
        }

        Imgui.Checkbox("ID Buttons", ref _showIdButtons);
        Imgui.SameLine(0, 10);
        Imgui.Checkbox("Wiki Links", ref _showEncyclopediaButtons);
        Imgui.SameLine(0, 10);
        Imgui.Checkbox("Auto-Select on Hover", ref _autoSelectOnHover);
        Imgui.Separator();

        Collapse(
            "SELECTED PARTY",
            () =>
            {
                if (_mobileParty == null)
                    Imgui.Text("<Select a party to see details>");
                else
                    DisplayPartyInfo(_mobileParty);
            }
        );

        Imgui.Separator();
        Collapse(
            "SELECT PARTY ...",
            () =>
            {
                Text("Select a party from the list below.", GrayStyleColor);
                Imgui.NewLine();

                Collapse(
                    $"Lords ({MobileParty.AllLordParties.Count})",
                    () => PartyCheckboxList(MobileParty.AllLordParties)
                );
                Collapse(
                    $"Bandits ({MobileParty.AllBanditParties.Count})",
                    () => PartyCheckboxList(MobileParty.AllBanditParties)
                );
                Collapse(
                    $"Caravans ({MobileParty.AllCaravanParties.Count})",
                    () => PartyCheckboxList(MobileParty.AllCaravanParties)
                );
                Collapse(
                    $"Villagers ({MobileParty.AllVillagerParties.Count})",
                    () => PartyCheckboxList(MobileParty.AllVillagerParties)
                );
                Collapse(
                    $"Custom ({MobileParty.AllCustomParties.Count})",
                    () => PartyCheckboxList(MobileParty.AllCustomParties)
                );
            }
        );
    }

    public void OnPartyHover(MobileParty party)
    {
        if (_autoSelectOnHover)
            _mobileParty = party;
    }

    private void DisplayPartyInfo(MobileParty party)
    {
        if (!party.IsActive)
        {
            Imgui.Text("<Party is not available>");
            return;
        }

        FieldInfo("", party.Name.ToString(), party.StringId, party.StringId);
        var action = CampaignUIHelper.GetMobilePartyBehaviorText(party);
        Imgui.Text(action);

        Imgui.Separator();
        if (_mobileParty == MobileParty.MainParty)
            PlayerPartyActions();
        else
            MobilePartyActions(party);
        Imgui.Separator();
        Imgui.NewLine();

        FieldInfo(
            "Leader",
            party.LeaderHero?.Name?.ToString(),
            party.LeaderHero?.StringId,
            party.LeaderHero?.StringId,
            party.LeaderHero?.EncyclopediaLink
        );
        FieldInfo(
            "Clan",
            party.ActualClan?.Name?.ToString(),
            party.ActualClan?.StringId,
            party.ActualClan?.StringId,
            party.ActualClan?.EncyclopediaLink
        );
        FieldInfo(
            "Kingdom",
            party.ActualClan?.Kingdom?.Name?.ToString(),
            party.ActualClan?.Kingdom?.StringId,
            party.ActualClan?.Kingdom?.StringId,
            party.ActualClan?.Kingdom?.EncyclopediaLink
        );

        Imgui.Text(
            $"Total troops: {party.MemberRoster.TotalManCount} | Wounded: {party.MemberRoster.TotalWounded}"
        );
        Imgui.Text("Morale: " + party.Morale);
        Imgui.Text("Speed: " + party.Speed);
        Imgui.Text(
            $"Food: {party.Food} | Consumption: {party.FoodChange} | Days: {party.GetNumDaysForFoodToLast()}"
        );
        Imgui.Text($"Position: {party.Position.X}, {party.Position.Y}");
        FieldInfo(
            "Current Settlement",
            party.CurrentSettlement?.Name?.ToString(),
            party.CurrentSettlement?.StringId,
            party.CurrentSettlement?.StringId,
            party.CurrentSettlement?.EncyclopediaLink
        );

        Imgui.NewLine();

        AiBehaviorFor(party);

        Collapse(
            "Roles",
            () =>
            {
                FieldInfo(
                    "Scout",
                    party.EffectiveScout?.ToString(),
                    party.EffectiveScout?.StringId,
                    party.EffectiveScout?.StringId,
                    party.EffectiveScout?.EncyclopediaLink
                );
                FieldInfo(
                    "Quartermaster",
                    party.EffectiveQuartermaster?.ToString(),
                    party.EffectiveQuartermaster?.StringId,
                    party.EffectiveQuartermaster?.StringId,
                    party.EffectiveQuartermaster?.EncyclopediaLink
                );
                FieldInfo(
                    "Engineer",
                    party.EffectiveEngineer?.ToString(),
                    party.EffectiveEngineer?.StringId,
                    party.EffectiveEngineer?.StringId,
                    party.EffectiveEngineer?.EncyclopediaLink
                );
                FieldInfo(
                    "Surgeon",
                    party.EffectiveSurgeon?.ToString(),
                    party.EffectiveSurgeon?.StringId,
                    party.EffectiveSurgeon?.StringId,
                    party.EffectiveSurgeon?.EncyclopediaLink
                );
            }
        );

        Imgui.NewLine();
    }

    private void AiBehaviorFor(MobileParty party)
    {
        Collapse(
            "Ai Behavior",
            () =>
            {
                var partyAi = party.Ai;

                Imgui.Text("General");
                Imgui.Separator();
                Imgui.Text($"DefaultBehavior: {party.DefaultBehavior}");
                Imgui.Text($"PartyMoveMode: {party.PartyMoveMode}");
                Imgui.Text($"DoNotMakeNewDecisions: {partyAi.DoNotMakeNewDecisions}");

                Imgui.NewLine();

                Imgui.Text("Short Term");
                Imgui.Separator();
                Imgui.Text($"Behavior: {party.ShortTermBehavior}");
                var target = "<None>";
                if (party.ShortTermTargetParty != null)
                    target = party.ShortTermTargetParty.Name.ToString();
                else if (party.ShortTermTargetSettlement != null)
                    target = party.ShortTermTargetSettlement.Name.ToString();
                Text($"Target: {target}", target == "<None>" ? GrayStyleColor : null);

                Imgui.NewLine();
            }
        );
    }

    private void PlayerPartyActions()
    {
        Button("Heal (Cheat)", MobilePartyUtils.HealMainParty);
        Imgui.SameLine(0, 10);
        Button(
            (!MobileParty.MainParty.ShouldBeIgnored ? "Is attackable" : "Is not attackable")
                + " (Cheat)",
            MobilePartyUtils.ToggleMainPartyAttackable
        );
        Imgui.SameLine(0, 10);
        Button("Debug", () => Debug(MobileParty.MainParty), "Break into debugger (if attached)");
    }

    private void MobilePartyActions(MobileParty party)
    {
        if (MobileParty.MainParty.CurrentSettlement == null)
        {
            Button("Teleport to party", () => MobileParty.MainParty.TeleportTo(party));
            Imgui.SameLine(0, 10);
        }

        if (party.CurrentSettlement == null)
        {
            Button("Teleport to player", () => party.TeleportTo(MobileParty.MainParty));
            Imgui.SameLine(0, 10);
            Button("Destroy", party.Destroy);
            Imgui.SameLine(0, 10);
        }

        Button("Debug", () => Debug(party), "Break into debugger (if attached)");
    }

    private void Debug(MobileParty target)
    {
        var mobileParty = target;
        var partyBase = target.Party;

        Debugger.Break();
    }

    private void PartyCheckboxList(List<MobileParty> parties)
    {
        foreach (var party in parties.OrderBy(it => it.Name.ToString()))
        {
            PartyCheckbox(party);
        }
    }

    private void PartyCheckbox(MobileParty party)
    {
        var isSelected = party == _mobileParty;
        var partyLabel = $"{party.Name} ({party.MemberRoster.TotalManCount})##{party.StringId}";

        Imgui.Checkbox(partyLabel, ref isSelected);
        if (isSelected)
            _mobileParty = party;
    }

    private static Vec3 UnpackColor(uint color)
    {
        var r = ((color >> 16) & 0xFF) / 255f;
        var g = ((color >> 8) & 0xFF) / 255f;
        var b = (color & 0xFF) / 255f;

        const float mult = 1.5f; // Make colors brighter for better visibility
        return new Vec3(r * mult, g * mult, b * mult, 1);
    }

    private void FieldInfo(
        string label = "",
        string value = "",
        string id = "",
        string textToCopy = "",
        string encyclopediaLink = ""
    )
    {
        if (!string.IsNullOrEmpty(label))
        {
            Imgui.Text($"{label}:");
            Imgui.SameLine(0, 10);
        }

        if (string.IsNullOrEmpty(value))
        {
            Text("<None>", GrayStyleColor);

            return;
        }

        Imgui.Text(value);

        // Add ID button
        if (_showIdButtons && !string.IsNullOrEmpty(id))
        {
            Imgui.SameLine(0, 10);
            SmallButton(
                $"[{id}]##{id}{label}",
                () =>
                {
                    Clipboard.SetText(textToCopy);
                    InformationManager.DisplayMessage(
                        new InformationMessage($"Copied '{textToCopy}' to clipboard")
                    );
                },
                GrayStyleColor
            );
        }

        // Add encyclopedia link button
        // Bandits don't have encyclopedia pages
        if (
            _showEncyclopediaButtons
            && !string.IsNullOrEmpty(encyclopediaLink)
            && !_mobileParty.IsBandit
        )
        {
            Imgui.SameLine(0, 7);
            SmallButton(
                $"Wiki##{label}{encyclopediaLink}",
                () => Campaign.Current.EncyclopediaManager.GoToLink(encyclopediaLink)
            );
        }
    }
}

[HarmonyPatch(typeof(MobilePartyVisual), nameof(MobilePartyVisual.OnHover))]
public class MobilePartyHoverPatch
{
    public static void Postfix(MobileParty __instance)
    {
        var debuggers = WindowManager.GetAllWindows<MobilePartyDebugger>();
        foreach (var debugger in debuggers)
        {
            debugger.OnPartyHover(__instance);
        }
    }
}
