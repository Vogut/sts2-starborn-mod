using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Element;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Cards.Common;

[RegisterCard(typeof(StarbornCardPool))]
public sealed class WolfTotemCard() : StarbornCard(
    2, CardType.Skill, CardRarity.Common, TargetType.Self
)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(5m, ValueProp.Move),
        new IntVar("Turns", 2),
        StarbornCardVars.ElementMark(2, SealElementType.Water),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<WolfTotemPower>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

        var power = await PowerCmd.Apply<WolfTotemPower>(
            choiceContext, Owner.Creature, DynamicVars["Turns"].IntValue, Owner.Creature, this);
        if (power == null) return;

        power.DynamicVars.Block.BaseValue = decimal.Max(
            power.DynamicVars.Block.BaseValue, DynamicVars.Block.BaseValue);
        power.DynamicVars["ElementMark"].BaseValue = decimal.Max(
            power.DynamicVars["ElementMark"].BaseValue, DynamicVars["ElementMark"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}
