using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Commands;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Cards.Rare;

[RegisterCard(typeof(StarbornCardPool))]
public class OneQTwoQThreeQCard() : StarbornCard(
    2, CardType.Power, CardRarity.Rare, TargetType.Self
)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Interval", 4),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<OneQTwoQThreeQPower>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var interval = DynamicVars["Interval"].IntValue;
        await PowerCmd.Apply<OneQTwoQThreeQPower>(choiceContext, Owner.Creature, interval, Owner.Creature, this);
        await KiboCmd.SwitchOnAll(choiceContext, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Interval"].BaseValue -= 1;
    }
}
