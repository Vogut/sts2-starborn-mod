using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2_Starborn.Hooks;

namespace STS2_Starborn.Commands;

public static class KiboCmd
{
    public static async Task AutoPlay(PlayerChoiceContext ctx, CardModel card, ICombatState combatState)
    {
        await KiboHooks.BeforeKiboCardAutoPlay(combatState, card);
        await CardCmd.AutoPlay(ctx, card, null);
        await KiboHooks.AfterKiboCardAutoPlay(combatState, card);
    }
}
