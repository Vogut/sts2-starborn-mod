using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace STS2_Starborn.Hooks;

public static class KiboHooks
{
    public static async Task BeforeKiboCardAutoPlay(ICombatState combatState, CardModel card)
    {
        foreach (var model in combatState.IterateHookListeners())
        {
            if (model is IKiboCardPlayListener listener)
                await listener.BeforeKiboCardAutoPlayed(card);
            model.InvokeExecutionFinished();
        }
    }

    public static async Task AfterKiboCardAutoPlay(ICombatState combatState, CardModel card)
    {
        foreach (var model in combatState.IterateHookListeners())
        {
            if (model is IKiboCardPlayListener listener)
                await listener.AfterKiboCardAutoPlayed(card);
            model.InvokeExecutionFinished();
        }
    }
}
