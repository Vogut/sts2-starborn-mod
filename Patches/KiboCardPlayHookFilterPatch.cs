using System.Reflection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Patching.Models;
using STS2_Starborn.Cards.Kibo;

namespace STS2_Starborn.Patches;

public sealed class KiboCardPlayHookFilterPatch : IPatchMethod
{
    public static string PatchId => "sts2_starborn_kibo_card_play_hook_filter";
    public static string Description => "Suppress Hook.BeforeCardPlayed and Hook.AfterCardPlayed for Kibo pile cards";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets()
    {
        return
        [
            new(typeof(Hook), nameof(Hook.BeforeCardPlayed),
                [typeof(ICombatState), typeof(CardPlay)]),
            new(typeof(Hook), nameof(Hook.AfterCardPlayed),
                [typeof(ICombatState), typeof(PlayerChoiceContext), typeof(CardPlay)]),
        ];
    }

    public static bool Prefix(MethodBase __originalMethod, object[] __args, ref Task __result)
    {
        var cardPlay = (CardPlay)__args[^1];
        if (cardPlay.Card.HasModKeyword(KiboKeywords.PileMemberKeywordId))
        {
            __result = Task.CompletedTask;
            return false;
        }

        return true;
    }
}
