using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Element;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Cards.Rare;

[RegisterCard(typeof(StarbornCardPool))]
public sealed class TanaFlameRitualCard() : StarbornCard(
    3, CardType.Skill, CardRarity.Rare, TargetType.Self
)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Turns", 3),
        StarbornCardVars.ElementMark(2, SealElementType.Ice, "ElementMarkSecondary"),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<TanaFlameRitualPower>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await PowerCmd.Apply<TanaFlameRitualPower>(
            choiceContext, Owner.Creature, DynamicVars["Turns"].IntValue, Owner.Creature, this);
        if (power == null) return;

        power.DynamicVars["ElementMarkSecondary"].BaseValue = decimal.Max(
            power.DynamicVars["ElementMarkSecondary"].BaseValue,
            DynamicVars["ElementMarkSecondary"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ElementMarkSecondary"].UpgradeValueBy(1);
    }
}
