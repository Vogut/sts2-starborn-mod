using System;
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
using STS2_Starborn.Powers;

namespace STS2_Starborn.Cards.Uncommon;

[RegisterCard(typeof(StarbornCardPool))]
public sealed class NoBiggerCard() : StarbornCard(
    1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy
)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(7m, ValueProp.Move),
        new IntVar("Shrink", 1),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<ShrinkBuffPower>(),
        HoverTipFactory.FromPower<ShrinkDebuffPower>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        var shrinkStacks = DynamicVars["Shrink"].BaseValue;
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await PowerCmd.Apply<ShrinkBuffPower>(choiceContext,
            cardPlay.Target, shrinkStacks, Owner.Creature, this);
        await PowerCmd.Apply<ShrinkDebuffPower>(choiceContext,
            cardPlay.Target, shrinkStacks, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Shrink"].UpgradeValueBy(1m);
    }
}
