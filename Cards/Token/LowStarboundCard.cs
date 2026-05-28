using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2_Starborn.Character;
using STS2_Starborn.Commands;

namespace STS2_Starborn.Cards.Token;

[RegisterCard(typeof(StarbornCardPool))]
public sealed class LowStarboundCard() : StarbornCard(
    1, CardType.Skill, CardRarity.Token, TargetType.None
)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await KiboCmd.SummonRandom(choiceContext, Owner);

        var combatState = Owner!.Creature.CombatState;
        if (combatState == null) return;

        await KiboCmd.TryAutoPlayRandomNormalCard(Owner, combatState);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
