using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.CardPiles;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;

namespace STS2_Starborn.Kibo;

[RegisterOwnedCardPile("kibo_collection",
    Scope = ModCardPileScope.RunPersistent,
    Style = ModCardPileUiStyle.TopBarDeck,
    IconPath = Const.Paths.KiboCollectionPileIcon)]
public sealed class KiboCollectionPile
{
    public static string QualifiedPileId =>
        ModContentRegistry.GetQualifiedCardPileId(Const.ModId, "kibo_collection");

    public static async Task AddRepCard(Player player, KiboTypeId typeId)
    {
        var def = KiboTypeRegistry.Get(typeId);
        var combatState = player.Creature.CombatState;
        if (combatState == null)
            return;

        var pileType = ModCardPileRegistry.GetPileType(QualifiedPileId);
        var pile = pileType.GetPile(player);
        if (pile == null)
            return;

        if (pile.Cards.Any(c => c.GetType() == def.RepCardType))
            return;

        var canonical = ModelDb.GetById<CardModel>(ModelDb.GetId(def.RepCardType));
        var repCard = combatState.CreateCard(canonical, player);
        await CardPileCmd.Add(repCard, pile);
    }
}
