using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using STS2RitsuLib.Patching.Models;
using STS2_Starborn.UI;

namespace STS2_Starborn.Patches;

/// <summary>
/// Harmony Prefix on NHoverTipSet.Init() to intercept CompactCardGridHoverTip items
/// before the hard (CardHoverTip) cast crashes, rendering them in a compact 2-column grid.
/// </summary>
public sealed class CompactCardGridHoverTipPatch : IPatchMethod
{
    public static string PatchId => "sts2_starborn_compact_card_grid";
    public static string Description =>
        "Intercept CompactCardGridHoverTip in NHoverTipSet.Init() and render as 2-column grid";
    public static bool IsCritical => false;

    private static readonly FieldInfo? CardContainerField =
        AccessTools.Field(typeof(NHoverTipSet), "_cardHoverTipContainer");

    public static ModPatchTarget[] GetTargets()
    {
        return
        [
            new(typeof(NHoverTipSet), "Init",
                [typeof(Control), typeof(IEnumerable<IHoverTip>)]),
        ];
    }

    public static bool Prefix(NHoverTipSet __instance, Control owner,
        ref IEnumerable<IHoverTip> hoverTips)
    {
        var list = hoverTips.ToList();
        var compactTips = list.OfType<CompactCardGridHoverTip>().ToArray();
        if (compactTips.Length == 0)
            return true;

        list.RemoveAll(t => t is CompactCardGridHoverTip);
        hoverTips = list;

        var cardContainer = CardContainerField?.GetValue(__instance)
            as NHoverTipCardContainer;
        if (cardContainer == null || !GodotObject.IsInstanceValid(cardContainer))
            return true;

        foreach (var compact in compactTips)
        {
            var grid = new NCompactCardGrid();
            cardContainer.AddChild(grid);
            grid.Setup(compact.Cards);
        }

        return true;
    }
}
