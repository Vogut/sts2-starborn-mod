using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Patching.Models;
using STS2_Starborn.Cards;

namespace STS2_Starborn.Patches;

public sealed class StarbornElementMarkHoverTipPatch : IPatchMethod
{
    public static string PatchId => "sts2_starborn_element_mark_hover_tips";
    public static string Description => "Append tuning and overload hover tips for Starborn element mark card variables";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets()
    {
        return
        [
            new(typeof(CardModel), "HoverTips", MethodType.Getter),
        ];
    }

    public static void Postfix(CardModel __instance, ref IEnumerable<IHoverTip> __result)
    {
        var originalTips = __result.ToArray();
        var extraTips = StarbornElementHoverTipHelper.BuildElementMarkEffectTips(__instance, originalTips);
        if (extraTips.Length == 0)
            return;

        __result = IHoverTip.RemoveDupes(originalTips.Concat(extraTips)).ToArray();
    }
}
