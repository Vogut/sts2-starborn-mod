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
using STS2_Starborn.Combat;
using STS2_Starborn.Commands;
using STS2_Starborn.Element;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Cards.Common;

[RegisterCard(typeof(StarbornCardPool))]
public sealed class DragonTailSmashCard() : StarbornCard(
    0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy
)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        StarbornCardVars.Tuning(1, SealElementType.Water),
        StarbornCardVars.Overload(2, SealElementType.Water),
        StarbornCardVars.IfCanOverload(),
        new IntVar("Drown", 1),
        new DamageVar(4m, ValueProp.Move),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<DrownPower>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        var canOverload = StarbornCmd.CanOverload(Owner, MarkSlot.Primary);
        var elementType = canOverload
            ? ((SealElementVar)DynamicVars["Overload"]).ElementType
            : ((SealElementVar)DynamicVars["Tuning"]).ElementType;
        var consume = canOverload
            ? DynamicVars["Overload"].IntValue
            : DynamicVars["Tuning"].IntValue;

        if (canOverload)
        {
            await StarbornCmd.Overload(
                choiceContext, MarkSlot.Primary, Owner, consume, elementType, this, [cardPlay.Target]);
        }
        else
        {
            await StarbornCmd.Tuning(
                choiceContext, MarkSlot.Primary, Owner, consume, elementType, this, [cardPlay.Target]);
        }

        await PowerCmd.Apply<DrownPower>(
            choiceContext, cardPlay.Target, DynamicVars["Drown"].IntValue, Owner.Creature, this);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}
