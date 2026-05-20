using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Patching.Models;
using STS2_Starborn.Cards;

namespace STS2_Starborn.Kibo.Patches;

public sealed class KiboAutoSummonPatch : IPatchMethod
{
    public static string PatchId => "sts2_starborn_kibo_auto_summon";
    public static string Description => "Auto-summon Kibo when a card with KiboSummonType is played";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets()
    {
        return
        [
            new(typeof(CardModel), nameof(CardModel.OnPlayWrapper),
                [typeof(PlayerChoiceContext), typeof(Creature), typeof(bool), typeof(ResourceInfo), typeof(bool)]),
        ];
    }

    public static async void Postfix(CardModel __instance, PlayerChoiceContext choiceContext)
    {
        try
        {
            if (__instance is not StarbornCard starbornCard)
                return;
            if (starbornCard.KiboSummonType is not { } kiboType)
                return;

            await KiboSummonCmd.Summon(choiceContext, __instance.Owner, kiboType, __instance);
        }
        catch (Exception ex)
        {
            Entry.Logger.Error($"[KiboAutoSummon] Failed to summon: {ex}");
        }
    }
}
