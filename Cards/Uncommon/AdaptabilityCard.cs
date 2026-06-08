using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Combat;
using STS2_Starborn.Commands;
using STS2_Starborn.Element;

namespace STS2_Starborn.Cards.Uncommon;

[RegisterCard(typeof(StarbornCardPool))]
public sealed class AdaptabilityCard() : StarbornCard(
    0, CardType.Skill, CardRarity.Uncommon, TargetType.None
)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(1),
        Tuning(1, SealElementType.Any, "Tuning", MarkSlot.Primary),
        Overload(2, SealElementType.Any, "Overload", MarkSlot.Primary),
        Tuning(1, SealElementType.Any, "TuningSecondary", MarkSlot.Secondary),
        Overload(2, SealElementType.Any, "OverloadSecondary", MarkSlot.Secondary),
        StarbornCardVars.IfCanOverload(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var drawn = await CardPileCmd.Draw(choiceContext, Owner);
        if (drawn == null) return;

        if (drawn.Type == CardType.Attack)
        {
            var elementType = ((SealElementVar)DynamicVars["Tuning"]).ElementType;
            await StarbornCmd.Tuning(choiceContext, MarkSlot.Primary, Owner,
                DynamicVars["Tuning"].IntValue, elementType, this);
        }

        if (IsUpgraded && drawn.Type == CardType.Skill)
        {
            var elementType = ((SealElementVar)DynamicVars["TuningSecondary"]).ElementType;
            await StarbornCmd.Tuning(choiceContext, MarkSlot.Secondary, Owner,
                DynamicVars["TuningSecondary"].IntValue, elementType, this);
        }
    }

    protected override void OnUpgrade()
    {
        // Upgrade adds the Skill -> secondary Tuning branch in OnPlay
    }
}
