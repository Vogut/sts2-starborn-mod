using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.CardPiles;
using STS2RitsuLib.Content;
using STS2RitsuLib.Keywords;
using STS2_Starborn.Cards.Kibo;
using STS2_Starborn.Combat;

namespace STS2_Starborn.Cards.Pile;

public static class KiboPileManager
{
    public const string LocalPileStem = "kibo_pile";

    public static string QualifiedPileId =>
        ModContentRegistry.GetQualifiedCardPileId(Const.ModId, LocalPileStem);

    public static void RegisterPile()
    {
        ModCardPileRegistry.For(Const.ModId).RegisterOwned(LocalPileStem,
            new ModCardPileSpec
            {
                Scope = ModCardPileScope.CombatOnly,
                Style = ModCardPileUiStyle.BottomLeft,
                Anchor = new ModCardPileAnchor(ModCardPileAnchorKind.BottomLeftPrimary,
                    Offset: new Vector2(0, -80f)),
                IconPath = Const.Paths.KiboPileIcon,
            });
    }

    public static PileType GetPileType()
    {
        return ModCardPileRegistry.GetPileType(QualifiedPileId);
    }

    public static CardPile? GetPile(Player player)
    {
        if (player.Creature.CombatState == null)
            return null;

        var pileType = GetPileType();
        return pileType.GetPile(player);
    }

    public static async Task RefillPile(PlayerChoiceContext ctx, Player player, KiboTypeId typeId)
    {
        var pile = GetPile(player);
        if (pile == null)
            return;

        foreach (var card in pile.Cards.ToList())
            card.RemoveFromCurrentPile();

        var def = KiboTypeRegistry.Get(typeId);
        var combatState = player.Creature.CombatState!;

        foreach (var cardType in new[] { def.CardType1, def.CardType2 })
        {
            var canonical = ModelDb.GetById<CardModel>(ModelDb.GetId(cardType));
            var card = combatState.CreateCard(canonical, player);
            card.AddModKeyword(KiboKeywords.PileMemberKeywordId);
            await CardPileCmd.Add(card, pile);
        }

        KiboCombatManager.NotifyPileChanged();
    }
}
