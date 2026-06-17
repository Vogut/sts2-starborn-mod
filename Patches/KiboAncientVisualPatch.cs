using System;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using STS2RitsuLib.Patching.Models;
using STS2_Starborn.Cards;
using STS2_Starborn.Cards.Kibo;
using STS2_Starborn.Cards.Pile;

namespace STS2_Starborn.Patches;

/// <summary>
/// Makes selected cards display with Ancient card visuals while keeping their real rarity.
/// </summary>
public class KiboAncientVisualPatch : IPatchMethod
{
    public static string PatchId => "sts2_starborn_kibo_ancient_visual";
    public static string Description => "Make selected cards display with Ancient visual style";
    public static bool IsCritical => false;

    private const float KiboAncientTextBgAlpha = 0.45f;
    private static FieldInfo? _modelField;
    private static FieldInfo? _rarityField;

    public static ModPatchTarget[] GetTargets()
    {
        _modelField = AccessTools.Field(typeof(NCard), "_model");
        _rarityField = AccessTools.Field(typeof(CardModel), "<Rarity>k__BackingField");
        return
        [
            new(typeof(NCard), "Reload", [])
        ];
    }

    /// <summary>
    /// Temporarily changes selected cards' rarity to Ancient during Reload.
    /// </summary>
    public static void Prefix(NCard __instance, out AncientVisualState? __state)
    {
        __state = null;

        if (_modelField == null || _rarityField == null)
            return;

        var model = _modelField.GetValue(__instance) as CardModel;
        if (model == null)
            return;

        var visual = GetAncientVisual(model);
        if (visual == null)
            return;

        __state = new AncientVisualState(model.Rarity, visual.Value.TextBgAlpha);
        _rarityField.SetValue(model, CardRarity.Ancient);
    }

    /// <summary>
    /// Restores original rarity after Reload.
    /// </summary>
    public static void Postfix(NCard __instance, AncientVisualState? __state)
    {
        if (_modelField == null || _rarityField == null)
            return;

        var model = _modelField.GetValue(__instance) as CardModel;
        if (model == null)
            return;

        if (__state != null)
        {
            _rarityField.SetValue(model, __state.Value.OriginalRarity);
        }

        ApplyAncientTextBgOpacity(__instance, __state?.TextBgAlpha);
    }

    private static AncientVisualConfig? GetAncientVisual(CardModel model)
    {
        var attr = Attribute.GetCustomAttribute(model.GetType(), typeof(AncientVisualAttribute), false)
            as AncientVisualAttribute;
        if (attr != null)
            return new AncientVisualConfig(attr.TextBgAlpha);

        return IsKiboRepCard(model)
            ? new AncientVisualConfig(KiboAncientTextBgAlpha)
            : null;
    }

    private static bool IsKiboRepCard(CardModel? model)
    {
        if (model is not KiboCard)
            return false;

        return KiboPileManager.IsRepCardType(model.GetType());
    }

    private static void ApplyAncientTextBgOpacity(NCard card, float? textBgAlpha)
    {
        var textBg = card.GetNodeOrNull<CanvasItem>("%AncientTextBg");
        if (textBg == null)
            return;

        textBg.SelfModulate = textBgAlpha.HasValue
            ? new Color(1f, 1f, 1f, textBgAlpha.Value)
            : Colors.White;
    }

    public readonly record struct AncientVisualState(CardRarity OriginalRarity, float TextBgAlpha);

    private readonly record struct AncientVisualConfig(float TextBgAlpha);
}
