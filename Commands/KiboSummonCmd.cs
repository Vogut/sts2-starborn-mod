using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2_Starborn.Cards.Kibo;
using STS2_Starborn.Cards.Pile;
using STS2_Starborn.Runs;

namespace STS2_Starborn.Commands;

public static class KiboSummonCmd
{
    public static async Task Summon(PlayerChoiceContext ctx, Player player, KiboTypeId typeId)
    {
        KiboRunData.Modify(player, data =>
        {
            data.OwnedKiboTypeIds.Add(typeId.ToString());
            data.ActiveKiboTypeId = typeId.ToString();
        });

        await KiboCollectionPile.AddRepCard(player, typeId);

        if (!CombatManager.Instance.IsInProgress)
            return;

        await KiboPileManager.RefillPile(ctx, player, typeId);
    }
}
