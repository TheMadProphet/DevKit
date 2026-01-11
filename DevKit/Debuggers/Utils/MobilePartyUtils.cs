using System.Collections.Generic;
using SandBox.View.Map;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace DevKit.Debuggers.Utils;

public static class MobilePartyUtils
{
    public static void HealMainParty()
    {
        var result = CampaignCheats.HealMainParty([]);

        if (!string.IsNullOrEmpty(result))
            InformationManager.DisplayMessage(new InformationMessage("Cheat: " + result));
    }

    public static void ToggleMainPartyAttackable()
    {
        var result = CampaignCheats.SetMainPartyAttackable(
            MobileParty.MainParty.ShouldBeIgnored ? ["0"] : ["1"]
        );

        if (!string.IsNullOrEmpty(result))
            InformationManager.DisplayMessage(new InformationMessage("Cheat: " + result));
    }

    public static void TeleportTo(this MobileParty party, MobileParty destination)
    {
        var intersectionPoint = destination.Position;
        if (party.Army != null)
        {
            var attachedParties = party.Army.LeaderParty.AttachedParties;
            foreach (var mobileParty in attachedParties)
            {
                mobileParty.Position += intersectionPoint - party.Position;
            }
        }
        party.Position = intersectionPoint;
        party.SetMoveModeHold();

        foreach (var mobileParty in MobileParty.All)
            mobileParty.Party.UpdateVisibilityAndInspected(MobileParty.MainParty.Position);
        foreach (var settlement in Settlement.All)
            settlement.Party.UpdateVisibilityAndInspected(MobileParty.MainParty.Position);

        MapScreen.Instance.TeleportCameraToMainParty();
    }

    public static void Destroy(this MobileParty party)
    {
        DestroyPartyAction.Apply(null, party);
    }
}
