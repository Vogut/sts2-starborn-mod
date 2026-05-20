using System.Reflection;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Patching.Models;
using STS2_Starborn.Cards.Kibo;

namespace STS2_Starborn.Patches;

public sealed class KiboDamageModifierPatch : IPatchMethod
{
    public static string PatchId => "sts2_starborn_kibo_damage_modifier";
    public static string Description => "Block Strength/Dexterity/PenNib damage modifiers for Kibo cards";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets()
    {
        return
        [
            new(typeof(StrengthPower), nameof(StrengthPower.ModifyDamageAdditive)),
            new(typeof(DexterityPower), nameof(DexterityPower.ModifyBlockAdditive)),
            new(typeof(PenNib), nameof(PenNib.ModifyDamageMultiplicative)),
        ];
    }

    public static bool Prefix(MethodBase __originalMethod, object[] __args)
    {
        var cardSource = ExtractCardSource(__originalMethod, __args);
        if (cardSource != null && cardSource.HasModKeyword(KiboKeywords.PileMemberKeywordId))
            return false;
        return true;
    }

    public static void Postfix(MethodBase __originalMethod, object[] __args, ref decimal __result)
    {
        var cardSource = ExtractCardSource(__originalMethod, __args);
        if (cardSource == null || !cardSource.HasModKeyword(KiboKeywords.PileMemberKeywordId))
            return;

        if (__originalMethod.DeclaringType == typeof(PenNib))
            __result = 1m;
    }

    private static CardModel? ExtractCardSource(MethodBase method, object[] args)
    {
        var parameters = method.GetParameters();
        for (var i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].Name == "cardSource" && args[i] is CardModel cardSource)
                return cardSource;
        }

        return null;
    }
}
