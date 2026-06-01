using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Combat;
using STS2_Starborn.Commands;
using STS2_Starborn.Element;

namespace STS2_Starborn.Cards.Rare;

[RegisterCard(typeof(StarbornCardPool))]
public sealed class JadeTwigLeaningRoyalFanCard() : StarbornCard(
    3, CardType.Attack, CardRarity.Rare, TargetType.None
)
{
    protected override bool IsPlayable =>
        StarbornCmd.CanOverload(Owner, MarkSlot.Primary)
        && StarbornCmd.CanOverload(Owner, MarkSlot.Secondary);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        StarbornCardVars.Overload(1, SealElementType.Light, "Overload"),
        StarbornCardVars.Overload(1, SealElementType.Wind, "OverloadSecondary"),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var primaryElementType = ((SealElementVar)DynamicVars["Overload"]).ElementType;
        await StarbornCmd.Overload(choiceContext, MarkSlot.Primary, Owner,
            DynamicVars["Overload"].IntValue, primaryElementType, this);

        var secondaryElementType = ((SealElementVar)DynamicVars["OverloadSecondary"]).ElementType;
        await StarbornCmd.Overload(choiceContext, MarkSlot.Secondary, Owner,
            DynamicVars["OverloadSecondary"].IntValue, secondaryElementType, this);
    }

    protected override void AfterCloned()
    {
        base.AfterCloned();
        BaseReplayCount = 2;
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
