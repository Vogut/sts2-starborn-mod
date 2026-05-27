using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;
using STS2_Starborn.Cards.Kibo;
using STS2_Starborn.Cards.Pile;
using STS2_Starborn.Commands;

namespace STS2_Starborn.Powers;

[RegisterPower]
public class OneQTwoQThreeQPower : StarbornPower
{
    private int _cardsPlayed;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override int DisplayAmount
    {
        get
        {
            if (IsCanonical) return 0;
            var threshold = (int)Amount;
            if (threshold <= 0) return 0;
            return threshold - (_cardsPlayed % threshold);
        }
    }

    public override PowerAssetProfile AssetProfile => new(
        IconPath: Const.Paths.PowerIcon(GetType()),
        BigIconPath: Const.Paths.PowerBigIcon(GetType())
    );

    public override bool ShouldPlay(CardModel card, AutoPlayType _)
    {
        if (card.Owner?.Creature != Owner) return true;
        if (card.HasModKeyword(KiboKeywords.PileMemberKeywordId)) return true;
        return card.Type != CardType.Attack;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner?.Creature != Owner) return;
        if (cardPlay.Card.Type != CardType.Skill) return;

        var combatState = Owner.CombatState;
        if (combatState == null) return;

        var player = combatState.Players.FirstOrDefault(p => p.Creature == Owner);
        if (player == null) return;

        _cardsPlayed++;
        InvokeDisplayAmountChanged();

        var threshold = (int)Amount;

        await KiboCmd.TryAutoPlayRandomNormalCard(player, combatState);

        if (threshold > 0 && _cardsPlayed % threshold == 0)
            await KiboCmd.TryAutoPlayRandomUltimateCard(player, combatState);
    }

    public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side,
        IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (!participants.Contains(Owner)) return Task.CompletedTask;
        _cardsPlayed = 0;
        InvokeDisplayAmountChanged();
        return Task.CompletedTask;
    }

    public static async Task MoveAllKiboToActive(Player player)
    {
        var combatStorage = KiboPileManager.GetStorageCombatPile(player);
        var activePile = KiboPileManager.GetActivePile(player);
        if (combatStorage == null || activePile == null) return;

        foreach (var card in combatStorage.Cards
                     .Where(c => !KiboPileManager.IsRepCardType(c.GetType()))
                     .ToList())
        {
            await CardPileCmd.Add(card, activePile);
        }
    }
}
