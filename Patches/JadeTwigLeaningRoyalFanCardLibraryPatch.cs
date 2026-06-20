using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Patching.Models;
using STS2_Starborn.Cards.Rare;

namespace STS2_Starborn.Patches;

/// <summary>
/// The card library renders canonical cards, which never invoke AfterCloned().
/// Supply this card's intrinsic replay count for that read-only presentation path.
/// </summary>
public sealed class JadeTwigLeaningRoyalFanCardLibraryPatch : IPatchMethod
{
    public static string PatchId => "sts2_starborn_jade_twig_card_library_replay";
    public static string Description => "Show Jade Twig Leaning Royal Fan's replay keyword in the card library";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(CardModel), nameof(CardModel.GetEnchantedReplayCount)),
    ];

    public static bool Prefix(CardModel __instance, ref int __result)
    {
        if (__instance is JadeTwigLeaningRoyalFanCard && __instance.IsCanonical)
        {
            __result = JadeTwigLeaningRoyalFanCard.ReplayCount;
            return false;
        }

        return true;
    }
}
