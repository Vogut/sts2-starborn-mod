using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Combat;
using STS2_Starborn.Commands;
using STS2_Starborn.Element;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Cards.Common;

[RegisterCard(typeof(StarbornCardPool))]
public sealed class PhantomKickCard() : StarbornCard(
    1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy
)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        StarbornCardVars.Tuning(1, SealElementType.Fire),
        StarbornCardVars.Overload(2, SealElementType.Fire),
        StarbornCardVars.IfCanOverload(),
        new RepeatVar(4),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<BurnPower>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        var tuningElementType = ((SealElementVar)DynamicVars["Tuning"]).ElementType;
        await StarbornCmd.Tuning(choiceContext, MarkSlot.Primary, Owner,
            DynamicVars["Tuning"].IntValue, tuningElementType, this);

        var burnStacks = cardPlay.Target.GetPowerAmount<BurnPower>();
        if (burnStacks <= 0) return;

        await DamageCmd.Attack((decimal)burnStacks)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitCount(DynamicVars.Repeat.IntValue)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Repeat.UpgradeValueBy(1);
    }
}
