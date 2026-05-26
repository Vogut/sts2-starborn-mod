using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2_Starborn.Character;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Cards.Event;

[RegisterCard(typeof(StarbornCardPool))]
public sealed class SimulationCard() : StarbornCard(
    1, CardType.Power, CardRarity.Event, TargetType.Self
)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Innate];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<SimulationPower>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<SimulationPower>(
            choiceContext, Owner.Creature, 1, Owner.Creature, this);
        if (IsUpgraded)
            await CardPileCmd.Draw(choiceContext, 2, Owner);
    }

    protected override void OnUpgrade()
    {
    }
}
