using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Combat;
using STS2_Starborn.Commands;
using STS2_Starborn.Element;

namespace STS2_Starborn.Cards.Basic;

[RegisterCard(typeof(StarbornCardPool))]
[RegisterCharacterStarterCard(typeof(Starborn), 1, Order = 2)]
public sealed class ReturnSeaOfStarsCard() : StarbornCard(
    1, CardType.Skill, CardRarity.Basic, TargetType.Self
)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ElementMark(1, SealElementType.Any, "ElementMark", MarkSlot.Primary),
        ElementMark(1, SealElementType.Any, "ElementMarkSecondary", MarkSlot.Secondary),
        Tuning(1, SealElementType.Any, "Tuning", MarkSlot.Primary),
        Tuning(1, SealElementType.Any, "TuningSecondary", MarkSlot.Secondary),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await SealElementMarkCmd.GainElementMarks(
            choiceContext, MarkSlot.Primary, Owner, DynamicVars["ElementMark"].IntValue);
        await SealElementMarkCmd.GainElementMarks(
            choiceContext, MarkSlot.Secondary, Owner, DynamicVars["ElementMarkSecondary"].IntValue);

        var tuningElementType = ((SealElementVar)DynamicVars["Tuning"]).ElementType;
        await StarbornCmd.Tuning(choiceContext, MarkSlot.Primary, Owner,
            DynamicVars["Tuning"].IntValue, tuningElementType, this);

        var secondaryTuningElementType = ((SealElementVar)DynamicVars["TuningSecondary"]).ElementType;
        await StarbornCmd.Tuning(choiceContext, MarkSlot.Secondary, Owner,
            DynamicVars["TuningSecondary"].IntValue, secondaryTuningElementType, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Tuning"].UpgradeValueBy(-1);
        DynamicVars["TuningSecondary"].UpgradeValueBy(-1);
    }
}
