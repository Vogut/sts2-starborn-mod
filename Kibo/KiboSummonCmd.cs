using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Scaffolding.Characters;

namespace STS2_Starborn.Kibo;

public static class KiboSummonCmd
{
    public static async Task Summon(PlayerChoiceContext ctx, Player player, KiboTypeId typeId, CardModel? source)
    {
        KiboRunData.Modify(player, data =>
        {
            data.OwnedKiboTypeIds.Add(typeId.ToString());
            data.ActiveKiboTypeId = typeId.ToString();
        });

        await KiboCollectionPile.AddRepCard(player, typeId);

        if (!CombatManager.Instance.IsInProgress)
            return;

        await ActivateInCombat(ctx, player, typeId, source);
    }

    public static async Task ActivateInCombat(PlayerChoiceContext ctx, Player player, KiboTypeId typeId, CardModel? source)
    {
        var existing = player.Creature.FindPower<KiboActivePower>();
        if (existing != null)
        {
            existing.SetActiveKiboType(typeId);
        }
        else
        {
            await PowerCmd.Apply<KiboActivePower>(ctx, player.Creature, 1, player.Creature, source);
            var power = player.Creature.FindPower<KiboActivePower>()!;
            power.SetActiveKiboType(typeId);
        }

        await KiboPileManager.RefillPile(ctx, player, typeId);
    }
}
