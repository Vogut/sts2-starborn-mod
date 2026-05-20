using System.Reflection;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Patching.Models;
using STS2_Starborn.Cards.Kibo;

namespace STS2_Starborn.Patches;

public sealed class KiboDamageReflectorPatch : IPatchMethod
{
    public static string PatchId => "sts2_starborn_kibo_damage_reflector";
    public static string Description => "Block Thorns/Reflect/FlameBarrier reflection for Kibo cards";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets()
    {
        return
        [
            new(typeof(ThornsPower), nameof(ThornsPower.BeforeDamageReceived)),
            new(typeof(ReflectPower), nameof(ReflectPower.AfterDamageReceived)),
            new(typeof(FlameBarrierPower), nameof(FlameBarrierPower.AfterDamageReceived)),
        ];
    }

    public static bool Prefix(MethodBase __originalMethod, object[] __args)
    {
        var cardSource = ExtractCardSource(__originalMethod, __args);
        if (cardSource != null && cardSource.HasModKeyword(KiboKeywords.PileMemberKeywordId))
            return false;
        return true;
    }

    public static void Postfix(MethodBase __originalMethod, object[] __args, ref Task __result)
    {
        if (__result != null)
            return;

        var cardSource = ExtractCardSource(__originalMethod, __args);
        if (cardSource != null && cardSource.HasModKeyword(KiboKeywords.PileMemberKeywordId))
            __result = Task.CompletedTask;
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
