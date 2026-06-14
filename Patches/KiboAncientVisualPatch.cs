using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using STS2RitsuLib.Patching.Models;
using STS2_Starborn.Cards.Kibo;
using STS2_Starborn.Cards.Pile;

namespace STS2_Starborn.Patches;

/// <summary>
/// Patch to make Kibo Rep cards display with Ancient card visuals while keeping Token rarity
/// 让奇波 Rep 卡以 Ancient 卡的视觉效果显示，但保持 Token 稀有度
/// Only applies to Rep cards (cards with [RegisterKibo] attribute), not ability cards
/// 仅应用于 Rep 卡（带有 [RegisterKibo] 属性的卡），不影响技能卡
/// </summary>
public class KiboAncientVisualPatch : IPatchMethod
{
    public static string PatchId => "sts2_starborn_kibo_ancient_visual";
    public static string Description => "Make Kibo Rep cards display with Ancient visual style";
    public static bool IsCritical => false;

    private static FieldInfo? _modelField;

    public static ModPatchTarget[] GetTargets()
    {
        _modelField = AccessTools.Field(typeof(NCard), "_model");
        return
        [
            new(typeof(NCard), "Reload", [])
        ];
    }

    /// <summary>
    /// Prefix to temporarily change Kibo Rep card rarity to Ancient during Reload
    /// </summary>
    public static void Prefix(NCard __instance, out CardRarity? __state)
    {
        __state = null;

        if (_modelField == null)
            return;

        var model = _modelField.GetValue(__instance) as CardModel;
        if (model == null)
            return;

        // Check if this is a Kibo Rep card
        if (IsKiboRepCard(model))
        {
            // Store original rarity
            __state = model.Rarity;

            // Temporarily change to Ancient for visual rendering
            // This is a hack but it's the cleanest way to make NCard render it as Ancient
            var rarityField = AccessTools.Field(typeof(CardModel), "<Rarity>k__BackingField");
            if (rarityField != null)
            {
                rarityField.SetValue(model, CardRarity.Ancient);
            }
        }
    }

    /// <summary>
    /// Postfix to restore original rarity after Reload
    /// </summary>
    public static void Postfix(NCard __instance, CardRarity? __state)
    {
        if (__state == null || _modelField == null)
            return;

        var model = _modelField.GetValue(__instance) as CardModel;
        if (model == null)
            return;

        // Restore original rarity
        var rarityField = AccessTools.Field(typeof(CardModel), "<Rarity>k__BackingField");
        if (rarityField != null)
        {
            rarityField.SetValue(model, __state.Value);
        }
    }

    /// <summary>
    /// Check if the card model is a Kibo Rep card (not an ability card)
    /// 检查卡牌是否为奇波 Rep 卡（而非技能卡）
    /// </summary>
    private static bool IsKiboRepCard(CardModel? model)
    {
        if (model is not KiboCard)
            return false;

        // Use KiboPileManager's method to check if it's a Rep card
        // Rep cards have the [RegisterKibo] attribute
        return KiboPileManager.IsRepCardType(model.GetType());
    }
}
