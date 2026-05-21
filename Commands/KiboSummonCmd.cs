using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2_Starborn.Cards.Kibo;
using STS2_Starborn.Cards.Pile;
using STS2_Starborn.Hooks;
using STS2_Starborn.Runs;

namespace STS2_Starborn.Commands;

public static class KiboSummonCmd
{
    public static async Task Summon(PlayerChoiceContext ctx, Player player, KiboTypeId typeId)
    {
        var data = KiboRunData.Get(player);
        var from = data?.ActiveKiboTypeId != null
            ? Enum.Parse<KiboTypeId>(data.ActiveKiboTypeId)
            : (KiboTypeId?)null;

        if (from != null && from.Value == typeId)
            return;

        var combatState = player.Creature.CombatState;
        if (from != null && combatState != null)
        {
            if (KiboHooks.AnyListenerPreventsKiboSwitch(combatState, player, from.Value, typeId))
                return;
            await KiboHooks.BeforeKiboSwitch(combatState, player, from.Value, typeId);
        }

        KiboRunData.Modify(player, data =>
        {
            data.OwnedKiboTypeIds.Add(typeId.ToString());
            data.ActiveKiboTypeId = typeId.ToString();
        });

        await KiboCollectionPile.AddRepCard(player, typeId);

        if (!CombatManager.Instance.IsInProgress)
            return;

        if (from != null && combatState != null)
            await KiboHooks.AfterKiboSwitch(combatState, player, from.Value, typeId);

        await KiboPileManager.RefillPile(ctx, player, typeId);
    }
}
