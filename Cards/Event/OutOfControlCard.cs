using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2_Starborn.Character;

namespace STS2_Starborn.Cards.Event;

/// <summary>
/// 失控：无法打出，保留。在回合结束（手牌中）和战斗结束时各失去 1 点生命上限。
/// </summary>
[RegisterCard(typeof(StarbornCardPool))]
public sealed class OutOfControlCard() : StarbornCard(
    -1, CardType.Curse, CardRarity.Curse, TargetType.None
)
{
    public override int MaxUpgradeLevel => 0;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Unplayable, CardKeyword.Retain, CardKeyword.Eternal];

    public override bool HasTurnEndInHandEffect => true;

    protected override async Task OnTurnEndInHand(PlayerChoiceContext choiceContext)
    {
        await CreatureCmd.LoseMaxHp(choiceContext, Owner.Creature, 1, isFromCard: true);
    }

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        await base.AfterCombatEnd(room);
        if (Owner?.Creature != null)
            await CreatureCmd.LoseMaxHp(
                new ThrowingPlayerChoiceContext(), Owner.Creature, 1, isFromCard: true);
    }
}
