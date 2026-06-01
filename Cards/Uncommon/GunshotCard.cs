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

namespace STS2_Starborn.Cards.Uncommon;

[RegisterCard(typeof(StarbornCardPool))]
public sealed class GunshotCard() : StarbornCard(
    1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy
)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        StarbornCardVars.Overload(1, SealElementType.Fire),
        new IntVar("Multiplier", 2),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<BurnPower>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        var overloadElementType = ((SealElementVar)DynamicVars["Overload"]).ElementType;
        await StarbornCmd.Overload(choiceContext, MarkSlot.Primary, Owner,
            DynamicVars["Overload"].IntValue, overloadElementType, this);

        var burnStacks = cardPlay.Target.GetPowerAmount<BurnPower>();
        if (burnStacks <= 0) return;

        var multiplier = DynamicVars["Multiplier"].IntValue;
        await DamageCmd.Attack((decimal)(burnStacks * multiplier))
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Multiplier"].UpgradeValueBy(1);
    }
}
