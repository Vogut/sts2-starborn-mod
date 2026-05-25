using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Keywords;
using STS2_Starborn.Cards.Kibo;
using STS2_Starborn.Cards.Pile;
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

    public static async Task<bool> TryAutoPlayRandomNormalCard(
        Player player, ICombatState combatState) =>
        await TryAutoPlayRandomCard(player, combatState, KiboKeywords.NormalKeywordId);

    public static async Task<bool> TryAutoPlayRandomUltimateCard(
        Player player, ICombatState combatState) =>
        await TryAutoPlayRandomCard(player, combatState, KiboKeywords.UltimateKeywordId);

    private static async Task<bool> TryAutoPlayRandomCard(
        Player player, ICombatState combatState, string keywordId)
    {
        var pile = KiboPileManager.GetActivePile(player);
        if (pile == null) return false;

        var cards = pile.Cards
            .Where(c => c.HasModKeyword(keywordId))
            .ToList();
        if (cards.Count == 0) return false;

        var card = cards[Random.Shared.Next(cards.Count)];

        await KiboHooks.BeforeKiboRandomAutoPlay(combatState, card, keywordId);
        await AutoPlay(new BlockingPlayerChoiceContext(), card, combatState);
        await KiboHooks.AfterKiboRandomAutoPlay(combatState, card, keywordId);

        return true;
    }
}
